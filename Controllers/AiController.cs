using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GatewayAI.Models;
using GatewayAI.Services;

namespace GatewayAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly ILlmService _llmService;
        private readonly AiRequestQueue _queue;
        private readonly GatewayContext _context;

        public AiController(ILlmService llmService, AiRequestQueue queue, GatewayContext context)
        {
            _llmService = llmService;
            _queue = queue;
            _context = context;
        }

        public class ChatRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public int? ConversationId { get; set; }
        }

        public class AnalyzeRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string TextToAnalyze { get; set; } = string.Empty;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var appName = HttpContext.Items["ApplicationName"]?.ToString() ?? "Unknown";
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Create conversation if not exists
                if (!request.ConversationId.HasValue)
                {
                    var newConversation = new Conversation
                    {
                        UserId = request.UserId,
                        ApplicationName = appName,
                        Summary = request.Prompt.Length > 50 ? request.Prompt.Substring(0, 50) + "..." : request.Prompt
                    };
                    _context.Conversations.Add(newConversation);
                    await _context.SaveChangesAsync();
                    request.ConversationId = newConversation.Id;
                }

                string analyzePrompt = $"Analyze the following text and extract key entities, sentiment, and summary:\n{request.Prompt}";
                // Gọi tới dịch vụ LLM thật. Tự xử lý Fallback và Cache bên trong
                var response = await _llmService.SendChatRequestAsync(request.Model, request.Prompt, request.UserId);
                stopwatch.Stop();

                // Calculate Cost (Bonus: Cost Estimation)
                decimal calculatedCost = (response.InputTokens / 1000m * 0.005m) + (response.OutputTokens / 1000m * 0.015m);

                // Bonus: Queue-based Processing
                _queue.Enqueue(new AiRequest
                {
                    UserId = request.UserId,
                    ApplicationName = appName,
                    Model = response.ModelUsed, 
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens,
                    Cost = calculatedCost,
                    Status = "Success",
                    ConversationId = request.ConversationId
                });

                return Ok(new 
                { 
                    text = response.Text, 
                    model_used = response.ModelUsed,
                    conversation_id = request.ConversationId,
                    cost = calculatedCost,
                    usage = new { input_tokens = response.InputTokens, output_tokens = response.OutputTokens } 
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Đưa lỗi vào hàng đợi log chạy nền
                _queue.Enqueue(new AiRequest
                {
                    UserId = request.UserId,
                    ApplicationName = appName,
                    Model = request.Model,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    InputTokens = 0,
                    OutputTokens = 0,
                    Cost = 0,
                    Status = "Error",
                    ErrorMessage = ex.Message,
                    ConversationId = request.ConversationId
                });

                return StatusCode(500, new { error = "AI Provider Error", details = ex.Message });
            }
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
        {
            var appName = HttpContext.Items["ApplicationName"]?.ToString() ?? "Unknown";
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                string analyzePrompt = $"Analyze the following text and extract key entities, sentiment, and summary:\n{request.TextToAnalyze}";
                var response = await _llmService.SendChatRequestAsync("gpt-3.5-turbo", analyzePrompt, request.UserId);
                stopwatch.Stop();

                decimal calculatedCost = (response.InputTokens / 1000m * 0.005m) + (response.OutputTokens / 1000m * 0.015m);

                _queue.Enqueue(new AiRequest
                {
                    UserId = request.UserId,
                    ApplicationName = appName,
                    Model = response.ModelUsed,
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens,
                    Cost = calculatedCost,
                    Status = "Success"
                });

                return Ok(new 
                { 
                    analysis_result = response.Text, 
                    model_used = response.ModelUsed,
                    cost = calculatedCost
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "AI Provider Error", details = ex.Message });
            }
        }
    }
}

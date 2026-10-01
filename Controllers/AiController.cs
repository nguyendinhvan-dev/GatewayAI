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

        public AiController(ILlmService llmService, AiRequestQueue queue)
        {
            _llmService = llmService;
            _queue = queue;
        }

        public class ChatRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var appName = HttpContext.Items["ApplicationName"]?.ToString() ?? "Unknown";
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Gọi tới dịch vụ LLM thật. Tự xử lý Fallback và Cache bên trong
                var response = await _llmService.SendChatRequestAsync(request.Model, request.Prompt, request.UserId);
                stopwatch.Stop();

                // Bonus: Queue-based Processing (Đưa việc ghi log xuống chạy nền)
                _queue.Enqueue(new AiRequest
                {
                    UserId = request.UserId,
                    ApplicationName = appName,
                    Model = response.ModelUsed, // Có thể khác model truyền vào nếu dùng Fallback
                    LatencyMs = stopwatch.ElapsedMilliseconds,
                    InputTokens = response.InputTokens,
                    OutputTokens = response.OutputTokens,
                    Status = "Success"
                });

                return Ok(new 
                { 
                    text = response.Text, 
                    model_used = response.ModelUsed,
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
                    Status = "Error",
                    ErrorMessage = ex.Message
                });

                return StatusCode(500, new { error = "AI Provider Error", details = ex.Message });
            }
        }
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GatewayAI.Services
{
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LlmService> _logger;
        private readonly IMemoryCache _cache;
        private readonly string? _openAiApiKey;
        private readonly string? _anthropicApiKey;

        public LlmService(HttpClient httpClient, IConfiguration config, ILogger<LlmService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _cache = cache;
            _openAiApiKey = config["AI_PROVIDERS:OPENAI_API_KEY"];
            _anthropicApiKey = config["AI_PROVIDERS:ANTHROPIC_API_KEY"];
        }

        public async Task<LlmResponse> SendChatRequestAsync(string model, string prompt, string userId)
        {
            // Bonus: Caching (nếu prompt đã được hỏi gần đây, trả về luôn từ cache để giảm cost/latency)
            var cacheKey = $"chat_{model}_{prompt.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out LlmResponse? cachedResponse) && cachedResponse != null)
            {
                _logger.LogInformation("Returning cached response for prompt");
                return cachedResponse;
            }

            // Bonus: Multi-model routing & Fallback
            // Nếu gọi model chính thất bại, tự động fallback sang model dự phòng
            try
            {
                LlmResponse response = await RouteToProvider(model, prompt);
                
                // Cache trong 5 phút
                _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary model {Model} failed. Attempting fallback.", model);
                
                // Fallback logic
                string fallbackModel = model.Contains("gpt") ? "claude-3-haiku-20240307" : "gpt-3.5-turbo";
                LlmResponse fallbackResponse = await RouteToProvider(fallbackModel, prompt);
                fallbackResponse.ModelUsed = fallbackModel; // Đánh dấu đã dùng fallback model
                return fallbackResponse;
            }
        }

        private async Task<LlmResponse> RouteToProvider(string model, string prompt)
        {
            if (model.Contains("gpt"))
            {
                return await CallOpenAiAsync(model, prompt);
            }
            else if (model.Contains("claude"))
            {
                return await CallAnthropicAsync(model, prompt);
            }
            else
            {
                throw new NotSupportedException($"Model {model} is not supported by Gateway.");
            }
        }

        private async Task<LlmResponse> CallOpenAiAsync(string model, string prompt)
        {
            if (string.IsNullOrEmpty(_openAiApiKey)) throw new InvalidOperationException("OpenAI API Key is missing");

            _logger.LogInformation("Calling OpenAI API: {Model}", model);
            var requestBody = new
            {
                model = model,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 1000
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var text = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            var usage = result.GetProperty("usage");

            return new LlmResponse
            {
                Text = text,
                InputTokens = usage.GetProperty("prompt_tokens").GetInt32(),
                OutputTokens = usage.GetProperty("completion_tokens").GetInt32(),
                ModelUsed = model
            };
        }

        private async Task<LlmResponse> CallAnthropicAsync(string model, string prompt)
        {
            if (string.IsNullOrEmpty(_anthropicApiKey)) throw new InvalidOperationException("Anthropic API Key is missing");

            _logger.LogInformation("Calling Anthropic API: {Model}", model);
            var requestBody = new
            {
                model = model,
                max_tokens = 1000,
                messages = new[] { new { role = "user", content = prompt } }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", _anthropicApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var text = result.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
            var usage = result.GetProperty("usage");

            return new LlmResponse
            {
                Text = text,
                InputTokens = usage.GetProperty("input_tokens").GetInt32(),
                OutputTokens = usage.GetProperty("output_tokens").GetInt32(),
                ModelUsed = model
            };
        }
    }
}

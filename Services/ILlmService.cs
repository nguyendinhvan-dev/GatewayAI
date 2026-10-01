using System.Threading.Tasks;

namespace GatewayAI.Services
{
    public interface ILlmService
    {
        Task<LlmResponse> SendChatRequestAsync(string model, string prompt, string userId);
    }

    public class LlmResponse
    {
        public string Text { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string ModelUsed { get; set; } = string.Empty;
    }
}

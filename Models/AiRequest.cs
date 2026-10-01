using System;

namespace GatewayAI.Models
{
    public class AiRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public long LatencyMs { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string Status { get; set; } = "Success"; // Success, Error, Timeout
        public string? ErrorMessage { get; set; }
        
        public int? ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
    }
}

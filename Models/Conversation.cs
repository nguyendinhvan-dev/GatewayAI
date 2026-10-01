using System;
using System.Collections.Generic;

namespace GatewayAI.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;

        public ICollection<AiRequest> Requests { get; set; } = new List<AiRequest>();
    }
}

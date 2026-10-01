using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GatewayAI.Data;

namespace GatewayAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly GatewayContext _context;

        public ConversationsController(GatewayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations([FromQuery] string userId)
        {
            var appName = HttpContext.Items["ApplicationName"]?.ToString() ?? "Unknown";

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId query parameter is required.");
            }

            var conversations = await _context.Conversations
                .Include(c => c.Requests)
                .Where(c => c.ApplicationName == appName && c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Summary,
                    c.CreatedAt,
                    RequestCount = c.Requests.Count
                })
                .ToListAsync();

            return Ok(conversations);
        }
    }
}

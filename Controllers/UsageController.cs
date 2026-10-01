using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GatewayAI.Data;

namespace GatewayAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsageController : ControllerBase
    {
        private readonly GatewayContext _context;

        public UsageController(GatewayContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsage()
        {
            var appName = HttpContext.Items["ApplicationName"]?.ToString();
            
            var requests = _context.AiRequests.AsQueryable();
            if (!string.IsNullOrEmpty(appName))
            {
                requests = requests.Where(r => r.ApplicationName == appName);
            }

            var totalRequests = await requests.CountAsync();
            var totalTokens = await requests.SumAsync(r => r.InputTokens + r.OutputTokens);
            var avgLatency = totalRequests > 0 ? await requests.AverageAsync(r => r.LatencyMs) : 0;
            var errorCount = await requests.CountAsync(r => r.Status != "Success");
            var errorRate = totalRequests > 0 ? (double)errorCount / totalRequests : 0;

            return Ok(new
            {
                requests = totalRequests,
                tokens = totalTokens,
                average_latency_ms = (long)avgLatency,
                error_rate = errorRate
            });
        }
    }
}

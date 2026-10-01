using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using GatewayAI.Data;
using GatewayAI.Models;

namespace GatewayAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GatewayContext _context;

        public AuthController(GatewayContext context)
        {
            _context = context;
        }

        public class AuthRequest
        {
            public string ApplicationName { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateKey([FromBody] AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApplicationName))
                return BadRequest("ApplicationName is required.");

            var apiKey = new ApiKey
            {
                Key = "key-" + Guid.NewGuid().ToString("N"),
                ApplicationName = request.ApplicationName
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return Ok(new { apiKey = apiKey.Key, application = apiKey.ApplicationName });
        }
    }
}

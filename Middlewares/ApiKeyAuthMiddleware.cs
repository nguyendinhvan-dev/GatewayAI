using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using GatewayAI.Data;

namespace GatewayAI.Middlewares
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-Api-Key";

        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, GatewayContext dbContext)
        {
            // Skip auth for Swagger or home page if needed, but here we require it for /api endpoints
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { Error = "API Key is missing." });
                    return;
                }

                var apiKey = await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Key == extractedApiKey.ToString() && k.IsActive);

                if (apiKey == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { Error = "Unauthorized client. Invalid API Key." });
                    return;
                }

                // Add ApplicationName to HttpContext items for later use in controllers
                context.Items["ApplicationName"] = apiKey.ApplicationName;
            }

            await _next(context);
        }
    }
}

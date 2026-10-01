using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using GatewayAI.Data;
using GatewayAI.Services;
using GatewayAI.Middlewares;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

// Add Caching (Bonus feature)
builder.Services.AddMemoryCache();

builder.Services.AddControllers();

// Configure SQLite DB
builder.Services.AddDbContext<GatewayContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=gateway.db"));

// Configure HTTP Client for LLM with Resilience (Retry & Timeout)
builder.Services.AddHttpClient<ILlmService, LlmService>(client => 
{
    // Default Timeout across all providers
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler(options => 
{
    // Retry up to 3 times on transient errors (like 429 Too Many Requests, or 5xx)
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.Delay = TimeSpan.FromSeconds(2);
});

// Configure Queue-based Processing (Bonus feature)
builder.Services.AddSingleton<AiRequestQueue>();
builder.Services.AddHostedService<AiRequestProcessor>();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Request.Headers["X-Api-Key"].ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // 100 requests per minute
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Migrate DB on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GatewayContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiter();

app.UseRouting();

app.UseMiddleware<ApiKeyAuthMiddleware>();

app.MapControllers();

app.Run();

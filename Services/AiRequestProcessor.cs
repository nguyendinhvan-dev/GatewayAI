using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GatewayAI.Data;

namespace GatewayAI.Services
{
    public class AiRequestProcessor : BackgroundService
    {
        private readonly AiRequestQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AiRequestProcessor> _logger;

        public AiRequestProcessor(AiRequestQueue queue, IServiceProvider serviceProvider, ILogger<AiRequestProcessor> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Queue Processor is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var request = await _queue.DequeueAsync(stoppingToken);
                
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<GatewayContext>();
                        context.AiRequests.Add(request);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background task.");
                }
            }
        }
    }
}

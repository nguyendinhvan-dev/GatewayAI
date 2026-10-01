using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GatewayAI.Models;

namespace GatewayAI.Services
{
    public class AiRequestQueue
    {
        private readonly ConcurrentQueue<AiRequest> _workItems = new ConcurrentQueue<AiRequest>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void Enqueue(AiRequest workItem)
        {
            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<AiRequest> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem!;
        }
    }
}

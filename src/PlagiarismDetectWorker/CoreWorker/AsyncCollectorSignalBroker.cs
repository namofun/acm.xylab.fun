using Microsoft.Azure.WebJobs;
using Plag.Backend.Jobs;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Worker
{
    internal class AsyncCollectorSignalBroker : ISignalBroker
    {
        private readonly IAsyncCollector<string> _asyncCollector;

        public AsyncCollectorSignalBroker(IAsyncCollector<string> asyncCollector)
        {
            _asyncCollector = asyncCollector;
        }

        public async Task FireAsync(string signal)
        {
            await _asyncCollector.AddAsync(signal);
            await _asyncCollector.FlushAsync();
        }
    }
}

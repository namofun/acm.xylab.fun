using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Jobs;

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

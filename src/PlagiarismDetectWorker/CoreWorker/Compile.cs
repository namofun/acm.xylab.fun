using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Jobs;
using Plag.Backend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Worker
{
    public class Compile
    {
        private readonly IJobContext _store;
        private readonly IConvertService2 _converter;
        private readonly ICompileService _compiler;

        public Compile(IJobContext store, IConvertService2 converter, ICompileService compiler)
        {
            _store = store;
            _converter = converter;
            _compiler = compiler;
        }

        [FunctionName("Compile")]
        public Task Run(
            [QueueTrigger(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] string queueMessage,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> compilationContinuation,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log,
            CancellationToken cancellationToken)
            => CompilationWorker.RunAsync(
                queueMessage,
                _store,
                _converter,
                _compiler,
                log,
                new AsyncCollectorSignalBroker(compilationContinuation),
                new AsyncCollectorSignalBroker(reportGenerator),
                cancellationToken);
    }
}

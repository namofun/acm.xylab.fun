using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Jobs;
using Plag.Backend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Worker
{
    public class Report
    {
        private readonly IJobContext _store;
        private readonly IConvertService2 _converter;
        private readonly ICompileService _compiler;
        private readonly IReportService _reporter;

        public Report(
            IJobContext store,
            IConvertService2 converter,
            ICompileService compiler,
            IReportService reporter)
        {
            _store = store;
            _compiler = compiler;
            _reporter = reporter;
            _converter = converter;
        }

        [FunctionName("Report")]
        public Task Run(
            [QueueTrigger(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] string reportRequest,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportContinuation,
            ILogger log,
            CancellationToken cancellationToken)
            => ReportWorker.RunAsync(
                reportRequest,
                _store,
                _converter,
                _compiler,
                _reporter,
                log,
                new AsyncCollectorSignalBroker(reportContinuation),
                cancellationToken);
    }
}
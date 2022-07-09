using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Jobs;

namespace Xylab.PlagiarismDetect.Worker
{
    public class Rescue
    {
        [FunctionName("Rescue")]
        [FunctionAuthorize("PlagiarismDetectSystem.All")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rescue")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log)
        {
            if (!req.IsAuthorized()) return req.Forbid();

            string rescueRecord =
                await RescueWorker.RunAsync(
                    new AsyncCollectorSignalBroker(submissionTokenizer),
                    new AsyncCollectorSignalBroker(reportGenerator),
                    log);

            return new OkObjectResult(new { status = 202, comment = "Rescue request received.", trace = rescueRecord });
        }
    }
}

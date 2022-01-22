using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Plag.Backend.Jobs;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Worker
{
    public class Rescue
    {
        [FunctionName("Rescue")]
        public async Task<IActionResult> Run(
            [HttpTrigger("post", Route = "rescue")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log)
        {
            string rescueRecord =
                await RescueWorker.RunAsync(
                    new AsyncCollectorSignalBroker(submissionTokenizer),
                    new AsyncCollectorSignalBroker(reportGenerator),
                    log);

            return new OkObjectResult(new { status = 202, comment = "Rescue request received.", trace = rescueRecord });
        }
    }
}

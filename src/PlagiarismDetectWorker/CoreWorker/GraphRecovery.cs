using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Jobs;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Worker
{
    public class GraphRecovery
    {
        private readonly IJobContext _store;

        public GraphRecovery(IJobContext store)
        {
            _store = store;
        }

        [FunctionName("GraphRecoveryScheduler")]
        [FunctionAuthorize("PlagiarismDetectSystem.All")]
        public static async Task<IActionResult> Schedule(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "graph-recovery/{setid}")] HttpRequest req,
            string setid,
            [Queue(Startup.GraphRecoveryQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> recoveryQueue)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            bool force = req.Query.TryGetValue("force", out var forced) && forced.Contains("true");
            GraphRecoveryTask task = new() { SetId = setid, Force = force };
            await recoveryQueue.AddAsync(task.ToJson());
            await recoveryQueue.FlushAsync();
            return new OkObjectResult(new { reason = "Graph recovery scheduled.", task });
        }

        [FunctionName("GraphRecoveryRunner")]
        public async Task Run(
            [QueueTrigger(Startup.GraphRecoveryQueue, Connection = "AzureWebJobsStorage")] string taskJson,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator,
            ILogger log)
        {
            GraphRecoveryTask task = taskJson.AsJson<GraphRecoveryTask>();
            if (await GraphRecoveryWorker.RunAsync(task.SetId, task.Force, _store, log))
            {
                await reportGenerator.AddAsync(CorrelationRecord.New("recovery"));
                await reportGenerator.FlushAsync();
            }
        }

        private class GraphRecoveryTask
        {
            public string SetId { get; set; }

            public bool Force { get; set; }
        }
    }
}

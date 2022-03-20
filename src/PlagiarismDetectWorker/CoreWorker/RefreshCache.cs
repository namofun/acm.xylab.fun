using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Worker
{
    public class RefreshCache
    {
        private readonly IJobContext _jobContext;

        public RefreshCache(IJobContext jobContext)
        {
            _jobContext = jobContext;
        }

        [FunctionName("RefreshCache")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "refresh-cache")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Refreshing cache for the system.");
            await _jobContext.RefreshCacheAsync();
            return new OkObjectResult(new { result = "ok" });
        }
    }
}

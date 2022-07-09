using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Jobs;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Worker
{
    public class Bootstrap
    {
        private readonly IJobContext _store;
        private readonly ICompileService _compiler;

        public Bootstrap(IJobContext store, ICompileService compiler)
        {
            _store = store;
            _compiler = compiler;
        }

        [FunctionName("Bootstrap")]
        [FunctionAuthorize("PlagiarismDetectSystem.All")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bootstrap")] HttpRequest req,
            ILogger log)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            await BootstrapWorker.RunAsync(_store, _compiler, log);
            return new OkObjectResult(new { status = 200, comment = "Bootstrapping finished." });
        }
    }
}

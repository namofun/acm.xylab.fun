using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
    [FunctionAuthorize("BricksService.Puppeteer")]
    public static class Function
    {
        [FunctionName("Puppeteer_UpdateBrowser")]
        public static async Task<ActionResult<RevisionInfo>> RunUpdateBrowser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Puppeteer/UpdateBrowser")] HttpRequest req,
            [Blob(BrowserDownloader.ChromiumBlobUrl, FileAccess.Write)] BlobClient blob,
            ILogger log)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            return await BrowserDownloader.UpdateAsync(blob, log, req.Query.ContainsKey("force"));
        }

        [FunctionName("Puppeteer_Convert")]
        public static async Task<IActionResult> RunConvert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Puppeteer/Convert")] HttpRequest req,
            [Blob(BrowserDownloader.ChromiumBlobUrl, FileAccess.Read)] BlobClient chromiumZipBlob,
            ILogger log)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            try
            {
                RenderOptions options = await RenderOptions.FromRequest(req);

                Browser browser =
                    await BrowserDownloader.LaunchAsync(
                        chromiumZipBlob,
                        options.IgnoreHttpsErrors);

                try
                {
                    (Stream content, string mimeType) =
                        await BrowserManipulator.ConvertAsync(
                            browser,
                            options,
                            log);

                    return new FileStreamResult(content, mimeType);
                }
                finally
                {
                    await browser.CloseAsync();
                    await browser.DisposeAsync();
                }
            }
            catch (ServiceException ex)
            {
                return new ObjectResult(new { comment = ex.Message })
                {
                    StatusCode = (int)ex.StatusCode,
                };
            }
        }
    }
}

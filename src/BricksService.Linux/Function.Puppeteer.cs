using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
    public static class Function
    {
        [FunctionName("Puppeteer_UpdateBrowser")]
        public static async Task<RevisionInfo> RunUpdateBrowser(
            [HttpTrigger("post", Route = "Puppeteer/UpdateBrowser")] HttpRequest req,
            [Blob(BrowserDownloader.ChromiumBlobUrl, FileAccess.Write)] BlobClient blob,
            ILogger log)
        {
            return await BrowserDownloader.UpdateAsync(blob, log, req.Query.ContainsKey("force"));
        }

        [FunctionName("Puppeteer_Convert")]
        public static async Task<IActionResult> RunConvert(
            [HttpTrigger("get", "post", Route = "Puppeteer/Convert")] HttpRequest req,
            [Blob(BrowserDownloader.ChromiumBlobUrl, FileAccess.Read)] BlobClient chromiumZipBlob,
            ILogger log)
        {
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

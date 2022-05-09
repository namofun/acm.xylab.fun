using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Runtime.InteropServices;
using Azure.Storage.Blobs;
using System.Net;

namespace Xylab.BricksService.Puppeteer
{
    public static class Function
    {
        private static async Task<Browser> GetBrowser(BlobClient browserPackage, RenderOptions options)
        {
            BrowserFetcher browserFetcher = new(new BrowserFetcherOptions()
            {
                CustomFileDownload = async (_, dest) => await browserPackage.DownloadToAsync(dest),
                Path = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.GetTempPath() : null,
            });

            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            string browserPath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);

            Browser browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = browserPath,
                IgnoreHTTPSErrors = options.IgnoreHttpsErrors,
            });

            return browser;
        }

        [FunctionName("Puppeteer_Convert")]
        public static async Task<IActionResult> RunConvert(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Puppeteer/Convert")] HttpRequest req,
            [Blob(Manage.ChromiumBlobUrl, FileAccess.Read)] BlobClient chromiumZipBlob,
            ILogger log)
        {
            try
            {
                RenderOptions options = await RenderOptions.FromRequest(req);
                await using Browser browser = await GetBrowser(chromiumZipBlob, options);

                var page = await browser.NewPageAsync();

                page.Console += (sender, e) => log.LogInformation("PAGE LOG: {log}", e.Message.Text);
                page.Error += (sender, e) => { log.LogError("Error event emitted: {msg}", e.Error); browser.CloseAsync(); };

                try
                {
                    log.LogInformation("Set browser viewport..");
                    await page.SetViewportAsync(options.ViewPort.Reassign());

                    if (options.EmulateScreenMedia)
                    {
                        log.LogInformation("Emulate @media screen..");
                        await page.EmulateMediaTypeAsync(PuppeteerSharp.Media.MediaType.Screen);
                    }

                    if (!string.IsNullOrEmpty(options.Html))
                    {
                        log.LogInformation("Set HTML..");
                        await page.SetContentAsync(options.Html, options.Goto.Reassign());
                    }
                    else
                    {
                        log.LogInformation("Goto url '{url}'..", options.Url);
                        await page.GoToAsync(options.Url, options.Goto.Reassign());
                    }

                    if (options.ScrollPage)
                    {
                        log.LogInformation("Scroll page..");
                        throw new NotImplementedException();
                    }

                    // FailedResponses

                    // FailEarly

                    Stream data;
                    log.LogInformation("Rendering..");
                    if (options.Output == OutputType.pdf)
                    {
                        if (options.Pdf.FullPage)
                        {
                            throw new NotImplementedException();
                        }

                        data = await page.PdfStreamAsync();
                        return new FileStreamResult(data, "application/pdf");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    throw new BricksException(ex, ex.ToString(), HttpStatusCode.InternalServerError);
                }
                finally
                {
                    await browser.CloseAsync();
                }
            }
            catch (BricksException ex)
            {
                return new ObjectResult(new { comment = ex.Message })
                {
                    StatusCode = (int)ex.StatusCode,
                };
            }
        }
    }
}

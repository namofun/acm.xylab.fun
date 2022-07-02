using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
    public class BrowserManipulator
    {
        public static async Task<ValueTuple<Stream, string>> ConvertAsync(
            Browser browser,
            RenderOptions options,
            ILogger logger)
        {
            Page page = await browser.NewPageAsync();
            page.Console += (sender, e) =>
            {
                logger.LogInformation("PAGE LOG: {log}", e.Message.Text);
            };

            page.Error += (sender, e) =>
            {
                logger.LogError("Error event emitted: {msg}", e.Error);
                browser.CloseAsync();
            };

            List<HttpActivity> failedResponses = new();
            page.RequestFailed += (sender, e) =>
            {
                failedResponses.Add(e.Request);
            };

            page.Response += (sender, e) =>
            {
                if (e.Response.Status >= HttpStatusCode.BadRequest)
                {
                    failedResponses.Add(e.Response);
                }
            };

            try
            {
                logger.LogInformation("Set browser viewport..");
                await page.SetViewportAsync(options.ViewPort);

                if (options.EmulateScreenMedia)
                {
                    logger.LogInformation("Emulate @media screen..");
                    await page.EmulateMediaTypeAsync(PuppeteerSharp.Media.MediaType.Screen);
                }

                /*
                if (options.Cookies != null && options.Cookies.Length > 0)
                {
                    log.LogInformation("Setting cookies..");

                    CDPSession client = await page.Target.CreateCDPSessionAsync();
                    await client.SendAsync("Network.enable");
                    await client.SendAsync("Network.setCookies", new { cookies = options.Cookies });
                }
                */

                if (!string.IsNullOrEmpty(options.Html))
                {
                    logger.LogInformation("Set HTML..");
                    await page.SetContentAsync(options.Html, options.Goto);
                }
                else
                {
                    logger.LogInformation("Goto url '{url}'..", options.Url);
                    await page.GoToAsync(options.Url, options.Goto);
                }

                if (options.WaitFor != null)
                {
                    throw new NotImplementedException();
                }

                if (options.ScrollPage)
                {
                    logger.LogInformation("Scroll page..");
                    await ScrollPage(page);
                }

                if (failedResponses.Count > 0)
                {
                    logger.LogWarning(
                        "Number of failed requests: {count}\r\n" +
                        "{requests}",
                        failedResponses.Count,
                        string.Join("\r\n", failedResponses.Select(e => $"- {e.Status} {e.Url}")));

                    if (options.FailEarly == FailureHandling.all)
                    {
                        throw new ServiceException(
                            $"{failedResponses.Count} requests have failed. " +
                            $"See server log for more details.",
                            HttpStatusCode.PreconditionFailed);
                    }
                }

                var activity = failedResponses.Cast<HttpActivity?>().LastOrDefault(e => e.Value.Url == options.Url);
                if (options.FailEarly == FailureHandling.page && activity?.Response?.Status != HttpStatusCode.OK)
                {
                    throw new ServiceException(
                        $"Request for {options.Url} did not directly succeed and returned status {activity?.Status}",
                        HttpStatusCode.PreconditionFailed);
                }

                logger.LogInformation("Rendering..");
                if (options.Output == OutputType.pdf)
                {
                    if (options.Pdf.FullPage)
                    {
                        options.Pdf.Height = (await GetFullPageHeight(page)).ToString();
                    }

                    Stream data = await page.PdfStreamAsync(options.Pdf);
                    return (data, "application/pdf");
                }
                else if (options.Output == OutputType.html)
                {
                    string innerHtml = await page.EvaluateExpressionAsync<string>("document.documentElement.innerHTML");
                    return (
                        new MemoryStream(Encoding.UTF8.GetBytes(innerHtml)),
                        "text/html");
                }
                else
                {
                    Stream data;
                    PuppeteerSharp.ScreenshotOptions opt = options.Screenshot;
                    if (options.Screenshot.Selector == null)
                    {
                        data = await page.ScreenshotStreamAsync(opt);
                    }
                    else
                    {
                        ElementHandle element = await page.QuerySelectorAsync(options.Screenshot.Selector);
                        if (element == null)
                        {
                            throw new Exception("Element not found.");
                        }

                        opt.FullPage = false;
                        data = await element.ScreenshotStreamAsync(opt);
                    }

                    return (data, "image/" + options.Screenshot.Type);
                }
            }
            catch (Exception ex)
            {
                throw new ServiceException(ex, ex.ToString(), HttpStatusCode.InternalServerError);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private static Task<double> GetFullPageHeight(Page page)
        {
            return page.EvaluateExpressionAsync<double>(
@"
                Math.max(
                  document.body.scrollHeight,
                  document.body.offsetHeight,
                  document.documentElement.clientHeight,
                  document.documentElement.scrollHeight,
                  document.documentElement.offsetHeight
                )
");
        }

        private static Task ScrollPage(Page page)
        {
            return page.EvaluateFunctionAsync(
@"
                const scrollInterval = 100;
                const scrollStep = Math.floor(window.innerHeight / 2);
                const bottomThreshold = 400;

                function bottomPos() {
                    return window.pageYOffset + window.innerHeight;
                }

                return new Promise((resolve, reject) => {
                    function scrollDown() {
                        window.scrollBy(0, scrollStep);

                        if (document.body.scrollHeight - bottomPos() < bottomThreshold) {
                            window.scrollTo(0, 0);
                            setTimeout(resolve, 500);
                            return;
                        }

                        setTimeout(scrollDown, scrollInterval);
                    }

                    setTimeout(reject, 30000);
                    scrollDown();
                });
");
        }
    }
}

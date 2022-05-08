using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
    public static class Manage
    {
        public const string ChromiumBlobUrl = "azure-pipelines-deploy/puppeteer-chromium-" + BrowserFetcher.DefaultChromiumRevision + ".zip";

        [FunctionName("Puppeteer_UpdateBrowser")]
        public static async Task<RevisionInfo> RunUpdateBrowser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Puppeteer/UpdateBrowser")] HttpRequest req,
            [Blob(ChromiumBlobUrl, FileAccess.Write)] BlobClient blob,
            ILogger log)
        {
            BrowserFetcher browserFetcher = new(new BrowserFetcherOptions
            {
                Path = Path.GetTempPath(),
                Host = Environment.GetEnvironmentVariable("PUPPETEER_DOWNLOAD_HOST")
            });

            RevisionInfo revision = browserFetcher.RevisionInfo(BrowserFetcher.DefaultChromiumRevision);
            if (req.Query.ContainsKey("force"))
            {
                log.LogInformation("Forcefully removing existing blob.");
                await blob.DeleteIfExistsAsync();
            }
            else if (await blob.ExistsAsync())
            {
                log.LogInformation("Package already exists. downloadUrl='{url1}', blobUrl='{url2}'", revision.Url, ChromiumBlobUrl);
                return revision;
            }

            log.LogInformation("Start downloading package. downloadUrl='{url1}', blobUrl='{url2}'", revision.Url, ChromiumBlobUrl);
            using HttpClient httpClient = new();
            using HttpResponseMessage resp = await httpClient.GetAsync(revision.Url);
            log.LogInformation("Response code = '{resp}'", resp.StatusCode);
            resp.EnsureSuccessStatusCode();
            using Stream stream = await resp.Content.ReadAsStreamAsync();
            await blob.UploadAsync(stream);

            return revision;
        }
    }
}

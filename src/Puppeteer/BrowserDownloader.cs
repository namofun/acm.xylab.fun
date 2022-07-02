using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
    public class BrowserDownloader
    {
        public const string ChromiumBlobUrl =
            "azure-pipelines-deploy/puppeteer-chromium-"
            + BrowserFetcher.DefaultChromiumRevision
            + ".zip";

        public static async Task<RevisionInfo> UpdateAsync(
            BlobClient blob,
            ILogger logger,
            bool forceUpdate)
        {
            BrowserFetcher browserFetcher = new(new BrowserFetcherOptions
            {
                Path = Path.GetTempPath(),
                Host = Environment.GetEnvironmentVariable("PUPPETEER_DOWNLOAD_HOST")
            });

            RevisionInfo revision = browserFetcher.RevisionInfo(BrowserFetcher.DefaultChromiumRevision);
            if (forceUpdate)
            {
                logger.LogInformation("Forcefully removing existing blob.");
                await blob.DeleteIfExistsAsync();
            }
            else if (await blob.ExistsAsync())
            {
                logger.LogInformation("Package already exists. downloadUrl='{url1}', blobUrl='{url2}'", revision.Url, ChromiumBlobUrl);
                return revision;
            }

            logger.LogInformation("Start downloading package. downloadUrl='{url1}', blobUrl='{url2}'", revision.Url, ChromiumBlobUrl);
            using HttpClient httpClient = new();
            using HttpResponseMessage resp = await httpClient.GetAsync(revision.Url);
            logger.LogInformation("Response code = '{resp}'", resp.StatusCode);
            resp.EnsureSuccessStatusCode();
            using Stream stream = await resp.Content.ReadAsStreamAsync();
            await blob.UploadAsync(stream);

            return revision;
        }

        public static async Task<Browser> LaunchAsync(
            BlobClient browserPackage,
            bool ignoreHttpsErrors)
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
                IgnoreHTTPSErrors = ignoreHttpsErrors,
            });

            return browser;
        }
    }
}

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SatelliteSite
{
    public class ConnectionCache
    {
        public AzureStorageAccount Acm { get; }
        public AzureStorageAccount AcmArchive { get; }
        public QueueServiceClient QueueService { get; }
        public string BlobCachePath { get; }

        public ConnectionCache(IConfiguration configuration, IHostEnvironment environment)
        {
            BlobServiceClient client = new(configuration.GetConnectionString("AzureStorageAccount"));
            BlobCachePath = Path.Combine(environment.ContentRootPath, "blobcache");

            if (!Directory.Exists(BlobCachePath))
            {
                Directory.CreateDirectory(BlobCachePath);
            }

            QueueService = new(
                configuration.GetConnectionString("AzureStorageAccount"),
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });

            Acm = new(
                client.GetBlobContainerClient("acm"),
                BlobCachePath);

            AcmArchive = new(
                client.GetBlobContainerClient("acm-archive"),
                BlobCachePath,
                false,
                AccessTier.Cool);
        }
    }
}

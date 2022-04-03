using Ccs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Plag.Backend;
using Polygon;
using SatelliteSite.ContestModule.Components.ContestStatistics;
using SatelliteSite.ContestModule.Routing;
using SatelliteSite.OjUpdateModule.Services;

namespace SatelliteSite
{
    public class ConfigureConnections :
        IConfigureOptions<PolygonPhysicalOptions>,
        IConfigureOptions<ContestFileOptions>,
        IConfigureOptions<PlagBackendCosmosOptions>,
        IConfigureOptions<MinimalSiteOptions>,
        IConfigureOptions<ContestStatisticsOptions>,
        IConfigureOptions<RecordV2Options>,
        IConfigureOptions<AzureBlobWwwrootOptions>
    {
        private readonly ConnectionCache _connections;
        private readonly IConfiguration _configuration;

        public ConfigureConnections(ConnectionCache connectionCache, IConfiguration configuration)
        {
            _configuration = configuration;
            _connections = connectionCache;
        }

        public void Configure(ContestFileOptions options)
        {
            options.ContestFileProvider = _connections.BlobContainer_Acm;
        }

        public void Configure(PolygonPhysicalOptions options)
        {
            options.ProblemFileProvider = _connections.BlobContainer_Acm;
            options.JudgingFileProvider = _connections.BlobContainer_AcmArchive;
        }

        public void Configure(AzureBlobWwwrootOptions options)
        {
            options.LocalCachePath = _connections.BlobCachePath;
            options.BlobContainerClient = _connections.BlobContainer_Acm.ContainerClient;
            options.AllowedFolders = new[] { "/images/" };
        }

        public void Configure(PlagBackendCosmosOptions options)
        {
            options.DatabaseName = "PlagDetect";
            options.ConnectionString = _configuration.GetConnectionString("CosmosDbAccount");
        }

        public void Configure(MinimalSiteOptions options)
        {
            options.Keyword = _configuration.GetConnectionString("ContestKeyword");
        }

        public void Configure(ContestStatisticsOptions options)
        {
            options.DefaultContest = 13;
        }

        public void Configure(RecordV2Options options)
        {
            options.ConnectionString = _configuration.GetConnectionString("CosmosDbAccount");
        }
    }
}

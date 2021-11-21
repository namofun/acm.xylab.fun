using Microsoft.Extensions.Logging;
using SatelliteSite.OjUpdateModule.Entities;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.OjUpdateModule.Services
{
    public class CfUpdateServiceV2 : CfUpdateService
    {
        public CfUpdateServiceV2(
            ILogger<CfUpdateServiceV2> logger,
            IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        protected override async Task UpdateOne(
            HttpClient httpClient,
            SolveRecord id,
            CancellationToken stoppingToken)
        {
            int attempt = 0;

            do
            {
                try
                {
                    await base.UpdateOne(httpClient, id, stoppingToken);
                    attempt = int.MaxValue;
                }
                catch (System.Text.Json.JsonException)
                {
                    attempt++;
                    await Task.Delay(1000, stoppingToken);
                }
            }
            while (attempt >= 3);
        }
    }
}

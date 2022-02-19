#nullable enable
using Microsoft.Extensions.Logging;
using SatelliteSite.OjUpdateModule.Entities;
using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.OjUpdateModule.Services
{
    /// <summary>
    /// The update service for <see cref="RecordType.Codeforces"/>.
    /// </summary>
    public class CfUpdateService : OjUpdateService
    {
        /// <summary>
        /// Construct a <see cref="CfUpdateService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public CfUpdateService(
            ILogger<CfUpdateService> logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider,
                   RecordType.Codeforces, "Codeforces",
                   "http://codeforces.com/profile/{0}")
        {
            ColumnName = "Rating";
        }

        private class Rootobject
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("result")]
            public ResultValue[]? Result { get; set; }

            internal class ResultValue
            {
                [JsonPropertyName("rating")]
                public int? Rating { get; set; }

                [JsonPropertyName("handle")]
                public string? Handle { get; set; }
            }
        }

        /// <inheritdoc />
        public override string RankTemplate(int? rk)
        {
            if (!rk.HasValue) return "N/A";
            if (rk == -1000) return "Unrated";
            if (rk < 1200) return $"<b><font color=\"#808080\">{rk}</font></b>";
            if (rk < 1400) return $"<b><font color=\"#008000\">{rk}</font></b>";
            if (rk < 1600) return $"<b><font color=\"#03a89e\">{rk}</font></b>";
            if (rk < 1900) return $"<b><font color=\"#0000ff\">{rk}</font></b>";
            if (rk < 2100) return $"<b><font color=\"#a0a\">{rk}</font></b>";
            if (rk < 2400) return $"<b><font color=\"#ff8c00\">{rk}</font></b>";
            if (rk < 3000) return $"<b><font color=\"#ff0000\">{rk}</font></b>";
            return $"<b><font color=\"#aa0000\">{rk}</font></b>";
        }

        /// <inheritdoc />
        protected override void ConfigureHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://codeforces.com/");
            httpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        /// <inheritdoc />
        protected override string GenerateGetSource(string account)
        {
            return "api/user.info?handles=" + account;
        }

        /// <inheritdoc />
        protected override int? MatchCount(string html)
        {
            var obj = html.AsJson<Rootobject>();
            if (obj == null || obj.Status != "OK" || obj.Result?.Length != 1)
                return null; // User not ready?
            return obj.Result[0].Rating ?? -1000;
        }

        /// <inheritdoc />
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
                    if (attempt < 3)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            while (attempt >= 3);
        }
    }
}

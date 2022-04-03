#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xylab.BricksService.OjUpdate;

namespace SatelliteSite.XylabModule.Services
{
    /// <summary>
    /// The abstract base for external OJ updating.
    /// </summary>
    public class OjUpdateService : BackgroundService
    {
        private CancellationTokenSource? manualCancellatinSource;
        private bool firstUpdate = true;
        private readonly IServiceProvider _sp;
        private readonly ILogger _logger;

        /// <summary>
        /// The list of external OJ
        /// </summary>
        public static Dictionary<string, OjUpdateService> OjList { get; } = new();

        /// <summary>
        /// The sleep length for fetching span
        /// </summary>
        public static int SleepLength { get; set; }

        /// <summary>
        /// Whether this OJ update is being processed
        /// </summary>
        public bool IsUpdating { get; private set; }

        /// <summary>
        /// Last update time
        /// </summary>
        public DateTimeOffset? LastUpdate { get; private set; }

        /// <summary>
        /// Update driver
        /// </summary>
        public UpdateDriver Driver { get; }

        /// <summary>
        /// Construct the base of <see cref="OjUpdateService"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="updateDriver">The update driver.</param>
        public OjUpdateService(IServiceProvider serviceProvider, UpdateDriver updateDriver)
        {
            OjList[updateDriver.SiteName] = this;
            _sp = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SatelliteSite.XylabModule.Services.OjUpdateService." + updateDriver.Category);
            Driver = updateDriver;
        }

        /// <summary>
        /// Request the update and send the signal.
        /// </summary>
        public void RequestUpdate()
        {
            manualCancellatinSource?.Cancel();
        }

        /// <inheritdoc />
        /// <remarks>This operation will also load the last update time.</remarks>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            using var scope = _sp.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IConfigurationRegistry>();
            var confName = $"oj_{Driver.Category}_update_time";
            var conf = await registry.FindAsync(confName);

            if (conf == null)
            {
                _logger.LogError("No configuration added for OJ Update Service. Please check your migration.");
            }
            else
            {
                LastUpdate = conf.Value.AsJson<DateTimeOffset?>();
            }
        }

        /// <inheritdoc />
        /// <remarks>This operation will also store the last update time.</remarks>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            using var scope = _sp.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IConfigurationRegistry>();
            await registry.UpdateAsync($"oj_{Driver.Category}_update_time", LastUpdate.ToJson());
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Fetch service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IsUpdating = true;
                    manualCancellatinSource = null;
                    bool jumpFromUpdate = false;
                    int sleepLength = SleepLength * 60000;

                    if (firstUpdate)
                    {
                        firstUpdate = false;
                        jumpFromUpdate = true;
                    }

                    if (!jumpFromUpdate)
                    {
                        using var scope = _sp.CreateScope();
                        var store = scope.ServiceProvider.GetRequiredService<IRecordStorage>();
                        _logger.LogInformation("Fetch scope began!");
                        LastUpdate = await Driver.TryUpdateAsync(_logger, store, stoppingToken);
                        _logger.LogInformation("Fetch scope finished~");
                    }

                    // wait for task cancellation or next scope.
                    manualCancellatinSource = new CancellationTokenSource();
                    var chained = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, manualCancellatinSource.Token);
                    IsUpdating = false;
                    await Task.Delay(sleepLength, chained.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Fetch timer was interrupted.");
                }
            }

            _logger.LogDebug("Fetch service stopped.");
        }
    }
}

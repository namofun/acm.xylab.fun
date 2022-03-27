#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatelliteSite.OjUpdateModule.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.OjUpdateModule.Services
{
    /// <summary>
    /// The abstract base for external OJ updating.
    /// </summary>
    public abstract class OjUpdateService : BackgroundService
    {
        private CancellationTokenSource? manualCancellatinSource;
        private bool firstUpdate = true;

        /// <summary>
        /// The list of external OJ
        /// </summary>
        public static Dictionary<string, OjUpdateService> OjList { get; } = new Dictionary<string, OjUpdateService>();

        /// <summary>
        /// The sleep length for fetching span
        /// </summary>
        public static int SleepLength { get; set; }

        /// <summary>
        /// The site name of external OJ
        /// </summary>
        public string SiteName { get; }

        /// <summary>
        /// The HTML template to show ranks.
        /// </summary>
        /// <param name="rk">The rank value.</param>
        public abstract string RankTemplate(int? rk);

        /// <summary>
        /// The URL template for goto account page
        /// </summary>
        public string AccountTemplate { get; }

        /// <summary>
        /// The name displayed on web page
        /// </summary>
        public string ColumnName { get; protected set; }

        /// <summary>
        /// The category ID
        /// </summary>
        public RecordType CategoryId { get; }

        /// <summary>
        /// The dependency injection container
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Whether this OJ update is being processed
        /// </summary>
        public bool IsUpdating { get; private set; }

        /// <summary>
        /// Last update time
        /// </summary>
        public DateTimeOffset? LastUpdate { get; private set; }

        /// <summary>
        /// Construct the base of <see cref="OjUpdateService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="category">The category ID.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="siteName">The site name.</param>
        /// <param name="accountTemplate">The account template.</param>
        protected OjUpdateService(
            ILogger<OjUpdateService> logger,
            IServiceProvider serviceProvider,
            RecordType category,
            string siteName,
            string accountTemplate)
        {
            Logger = logger;
            SiteName = siteName;
            OjList[siteName] = this;
            CategoryId = category;
            ServiceProvider = serviceProvider;
            ColumnName = "Count";
            AccountTemplate = accountTemplate;
        }

        /// <summary>
        /// Request the update and send the signal.
        /// </summary>
        public void RequestUpdate()
        {
            manualCancellatinSource?.Cancel();
        }

        /// <summary>
        /// Initialize the <see cref="HttpClient"/>, setting the base url and timeout.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance.</param>
        protected abstract void ConfigureHttpClient(HttpClient httpClient);

        /// <summary>
        /// Create the HTTP GET url for account.
        /// </summary>
        /// <param name="account">The account name.</param>
        /// <returns>The GET source.</returns>
        protected abstract string GenerateGetSource(string account);

        /// <summary>
        /// Match the rating information.
        /// </summary>
        /// <param name="html">The http request body.</param>
        /// <returns>The count of solved problems.</returns>
        protected abstract int? MatchCount(string html);

        /// <summary>
        /// Update one record.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance.</param>
        /// <param name="id">The account information entity.</param>
        /// <param name="stoppingToken">The cancellation token.</param>
        protected virtual async Task UpdateOne(HttpClient httpClient, IRecord id, CancellationToken stoppingToken)
        {
            var getSrc = GenerateGetSource(id.Account);
            var resp = await httpClient.GetAsync(getSrc, stoppingToken);
            var result = await resp.Content.ReadAsStringAsync();
            id.Result = MatchCount(result);
        }

        /// <summary>
        /// Initialize the <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="handler">The instance of <see cref="HttpClientHandler"/>.</param>
        protected virtual void ConfigureHandler(HttpClientHandler handler)
        {
        }

        /// <inheritdoc />
        /// <remarks>This operation will also load the last update time.</remarks>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            using var scope = ServiceProvider.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IConfigurationRegistry>();
            var confName = $"oj_{CategoryId}_update_time";
            var conf = await registry.FindAsync(confName);

            if (conf == null)
            {
                Logger.LogError("No configuration added for OJ Update Service. Please check your migration.");
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

            using var scope = ServiceProvider.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IConfigurationRegistry>();
            await registry.UpdateAsync($"oj_{CategoryId}_update_time", LastUpdate.ToJson());
        }

        /// <summary>
        /// Try one update action.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>The task for updating.</returns>
        private async Task TryUpdateAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var handler = new HttpClientHandler();
                ConfigureHandler(handler);

                using var httpClient = new HttpClient();
                LastUpdate = null;
                ConfigureHttpClient(httpClient);

                using (var scope = ServiceProvider.CreateScope())
                {
                    var store = scope.ServiceProvider.GetRequiredService<ISolveRecordStore>();
                    var names = await store.GetAllAsync(CategoryId);

                    foreach (var id in names)
                    {
                        await UpdateOne(httpClient, id, stoppingToken);
                        await store.UpdateAsync(id, resultOnly: true);
                    }
                }

                LastUpdate = DateTimeOffset.Now;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Web request timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogWarning(ex, "Web request timed out.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Something wrong happend unexpectedly.");
            }
        }
        
        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogDebug("Fetch service started.");
            
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
                        Logger.LogInformation("Fetch scope began!");
                        await TryUpdateAsync(stoppingToken);
                        Logger.LogInformation("Fetch scope finished~");
                    }

                    // wait for task cancellation or next scope.
                    manualCancellatinSource = new CancellationTokenSource();
                    var chained = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, manualCancellatinSource.Token);
                    IsUpdating = false;
                    await Task.Delay(sleepLength, chained.Token);
                }
                catch (TaskCanceledException)
                {
                    Logger.LogWarning("Fetch timer was interrupted.");
                }
            }
            
            Logger.LogDebug("Fetch service stopped.");
        }
    }
}

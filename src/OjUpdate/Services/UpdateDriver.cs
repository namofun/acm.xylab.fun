#nullable enable
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The interface for external OJ updating.
    /// </summary>
    public interface IUpdateDriver
    {
        /// <summary>
        /// The site name of external OJ
        /// </summary>
        string SiteName { get; }

        /// <summary>
        /// The HTML template to show ranks.
        /// </summary>
        /// <param name="rk">The rank value.</param>
        string RankTemplate(int? rk);

        /// <summary>
        /// The URL template for goto account page
        /// </summary>
        string AccountTemplate { get; }

        /// <summary>
        /// The name displayed on web page
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// The category ID
        /// </summary>
        RecordType Category { get; }

        /// <summary>
        /// Try one update action.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="storage">The record storage.</param>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>The task for updating.</returns>
        Task<DateTimeOffset?> TryUpdateAsync(ILogger logger, IRecordStorage storage, CancellationToken stoppingToken = default);
    }

    /// <summary>
    /// The abstract base for external OJ updating.
    /// </summary>
    public abstract class UpdateDriver : IUpdateDriver
    {
        /// <inheritdoc />
        public abstract string SiteName { get; }

        /// <summary>
        /// The HTML template to show ranks.
        /// </summary>
        /// <param name="rk">The rank value.</param>
        public abstract string RankTemplate(int? rk);

        /// <summary>
        /// The URL template for goto account page
        /// </summary>
        public abstract string AccountTemplate { get; }

        /// <summary>
        /// The name displayed on web page
        /// </summary>
        public virtual string ColumnName => "Count";

        /// <summary>
        /// The category ID
        /// </summary>
        public abstract RecordType Category { get; }

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
        protected virtual async Task<int?> UpdateOne(HttpClient httpClient, IRecord id, CancellationToken stoppingToken)
        {
            var getSrc = GenerateGetSource(id.Account);
            using var resp = await httpClient.GetAsync(getSrc, stoppingToken);
            var result = await resp.Content.ReadAsStringAsync(stoppingToken);
            return MatchCount(result);
        }

        /// <summary>
        /// Initialize the <see cref="HttpClientHandler"/>.
        /// </summary>
        /// <param name="handler">The instance of <see cref="HttpClientHandler"/>.</param>
        protected virtual void ConfigureHandler(HttpClientHandler handler)
        {
        }

        /// <summary>
        /// Try one update action.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="storage">The record storage.</param>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>The task for updating.</returns>
        public async Task<DateTimeOffset?> TryUpdateAsync(ILogger logger, IRecordStorage storage, CancellationToken stoppingToken)
        {
            try
            {
                using var handler = new HttpClientHandler();
                ConfigureHandler(handler);

                using var httpClient = new HttpClient();
                ConfigureHttpClient(httpClient);

                var names = await storage.GetAllAsync(Category);
                foreach (var id in names)
                {
                    var res = await UpdateOne(httpClient, id, stoppingToken);
                    await storage.UpdateAsync(id, res);
                }

                return DateTimeOffset.Now;
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Web request timed out.");
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "Web request timed out.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something wrong happend unexpectedly.");
            }

            return null;
        }
    }
}

#nullable enable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The update service for <see cref="RecordType.Poj"/>.
    /// </summary>
    [Obsolete]
    public class PkuojDriver : UpdateDriver
    {
        public override string SiteName => "POJ";

        public override string AccountTemplate => "http://poj.org/userstatus?user_id={0}";

        public override RecordType Category => RecordType.Poj;

        /// <inheritdoc />
        public override string RankTemplate(int? rk)
        {
            return rk == null ? "N/A" : rk.Value.ToString();
        }

        /// <inheritdoc />
        protected override void ConfigureHandler(HttpClientHandler handler)
        {
            handler.CookieContainer = new System.Net.CookieContainer();
            handler.UseCookies = true;
        }

        /// <inheritdoc />
        protected override void ConfigureHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://poj.org/");
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
        }

        /// <inheritdoc />
        protected override string GenerateGetSource(string account)
        {
            return "userstatus?user_id=" + account;
        }

        /// <inheritdoc />
        protected override int? MatchCount(string html)
        {
            const string stt = "<td width=15% align=left>Solved:</td>\r\n" +
                "<td align=center width=25%><a href=status?result=0&user_id=";
            var sst = html.IndexOf(stt);
            // if (sst == -1) Logger.LogInformation(html);
            if (sst == -1) return default;

            var gt = html.IndexOf('>', sst + stt.Length) + 1;
            var lt = html.IndexOf('<', gt);
            var cnt = html[gt..lt];
            var success = int.TryParse(cnt, out int ans);
            return success ? ans : default(int?);
        }

        /// <inheritdoc />
        protected override async Task<int?> UpdateOne(
            HttpClient httpClient,
            IRecord id,
            CancellationToken stoppingToken)
        {
            int? res = await base.UpdateOne(httpClient, id, stoppingToken);
            await Task.Delay(10000, stoppingToken);
            return res;
        }
    }
}

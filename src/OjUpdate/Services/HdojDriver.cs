#nullable enable
using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Xylab.BricksService.OjUpdate
{
    /// <summary>
    /// The update service for <see cref="RecordType.Hdoj"/>.
    /// </summary>
    public class HdojDriver : UpdateDriver
    {
        public override string SiteName => "HDOJ";

        public override string AccountTemplate => "http://acm.hdu.edu.cn/userstatus.php?user={0}";

        public override RecordType Category => RecordType.Hdoj;

        /// <inheritdoc />
        public override string RankTemplate(int? rk)
        {
            return rk == null ? "N/A" : rk.Value.ToString();
        }

        /// <inheritdoc />
        protected override void ConfigureHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://acm.hdu.edu.cn/");
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <inheritdoc />
        protected override string GenerateGetSource(string account)
        {
            return "userstatus.php?user=" + account;
        }

        /// <inheritdoc />
        protected override int? MatchCount(string html)
        {
            var cnt = Regex.Match(html,
                @"<tr><td>Problems Solved</td><td align=center>(\S+)</td></tr>"
            ).Groups[1].Value;

            var success = int.TryParse(cnt, out int ans);
            return success ? ans : default(int?);
        }
    }
}

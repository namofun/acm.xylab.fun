using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using InteropNavigationOptions = PuppeteerSharp.NavigationOptions;
using WaitUntilNavigation = PuppeteerSharp.WaitUntilNavigation;

namespace Xylab.BricksService.Puppeteer
{
    public class GotoOptions
    {
        [JsonProperty("timeout")]
        public int? Timeout { get; set; }

        [JsonProperty("waitUntil")]
        public string WaitUntil { get; set; } = "networkidle0";

        private static readonly Dictionary<string, WaitUntilNavigation> map
            = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Load"] = WaitUntilNavigation.Load,
                ["DOMContentLoaded"] = WaitUntilNavigation.DOMContentLoaded,
                ["Networkidle0"] = WaitUntilNavigation.Networkidle0,
                ["Networkidle2"] = WaitUntilNavigation.Networkidle2,
            };

        public static implicit operator InteropNavigationOptions(GotoOptions @this)
        {
            InteropNavigationOptions options = new();
            options.Timeout = @this.Timeout;
            options.WaitUntil = @this.WaitUntil.Split(',').Select(e => map[e]).ToArray();
            return options;
        }
    }
}

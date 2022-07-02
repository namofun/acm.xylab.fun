using Newtonsoft.Json;
using InteropMarginOptions = PuppeteerSharp.Media.MarginOptions;

namespace Xylab.BricksService.Puppeteer
{
    public class MarginOptions
    {
        [JsonProperty("top")]
        public string Top { get; set; }

        [JsonProperty("right")]
        public string Right { get; set; }

        [JsonProperty("bottom")]
        public string Bottom { get; set; }

        [JsonProperty("left")]
        public string Left { get; set; }

        public static implicit operator InteropMarginOptions(MarginOptions @this)
        {
            return new()
            {
                Bottom = @this?.Bottom,
                Left = @this?.Left,
                Right = @this?.Right,
                Top = @this?.Top,
            };
        }
    }
}

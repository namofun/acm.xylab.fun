using Newtonsoft.Json;
using InteropScreenshotOptions = PuppeteerSharp.ScreenshotOptions;

namespace Xylab.BricksService.Puppeteer
{
    public class ScreenshotOptions
    {
        [JsonProperty("fullPage")]
        public bool? FullPage { get; set; }

        [JsonProperty("quality")]
        public int? Quality { get; set; }

        [JsonProperty("type")]
        public ScreenshotType Type { get; set; } = ScreenshotType.png;

        [JsonProperty("clip")]
        public ClipOptions Clip { get; set; }

        [JsonProperty("selector")]
        public string Selector { get; set; }

        [JsonProperty("omitBackground")]
        public bool OmitBackground { get; set; }

        public static implicit operator InteropScreenshotOptions(ScreenshotOptions @this)
        {
            return new InteropScreenshotOptions()
            {
                Type = (PuppeteerSharp.ScreenshotType)@this.Type,
                OmitBackground = @this.OmitBackground,
                Clip = @this.Clip,
                FullPage = @this.FullPage ?? false,
                Quality = @this.Quality,
            };
        }
    }
}

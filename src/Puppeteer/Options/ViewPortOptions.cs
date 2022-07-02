using Newtonsoft.Json;
using InteropViewPortOptions = PuppeteerSharp.ViewPortOptions;

namespace Xylab.BricksService.Puppeteer
{
    public class ViewPortOptions
    {
        [JsonProperty("width")]
        public int Width { get; set; } = 1600;

        [JsonProperty("height")]
        public int Height { get; set; } = 1200;

        [JsonProperty("deviceScaleFactor")]
        public double? DeviceScaleFactor { get; set; }

        [JsonProperty("isMobile")]
        public bool? IsMobile { get; set; }

        [JsonProperty("hasTouch")]
        public bool? HasTouch { get; set; }

        [JsonProperty("isLandscape")]
        public bool? IsLandscape { get; set; }

        public static implicit operator InteropViewPortOptions(ViewPortOptions @this)
        {
            InteropViewPortOptions options = new();
            options.Width = @this.Width;
            options.Height = @this.Height;
            options.DeviceScaleFactor = @this.DeviceScaleFactor ?? options.DeviceScaleFactor;
            options.IsMobile = @this.IsMobile ?? options.IsMobile;
            options.HasTouch = @this.HasTouch ?? options.HasTouch;
            options.IsLandscape = @this.IsLandscape ?? options.IsLandscape;
            return options;
        }
    }
}

using Newtonsoft.Json;
using Clip = PuppeteerSharp.Media.Clip;

namespace Xylab.BricksService.Puppeteer
{
    public class ClipOptions
    {
        [JsonProperty("x")]
        public decimal? X { get; set; }

        [JsonProperty("y")]
        public decimal? Y { get; set; }

        [JsonProperty("width")]
        public decimal? Width { get; set; }

        [JsonProperty("height")]
        public decimal? Height { get; set; }

        [JsonProperty("scale")]
        public int? Scale { get; set; }

        public static implicit operator Clip(ClipOptions @this)
        {
            if (@this == null
                || @this.Height == null
                || @this.Width == null
                || @this.X == null
                || @this.Y == null)
            {
                return null;
            }
            else
            {
                return new Clip
                {
                    Scale = @this.Scale ?? 1,
                    Height = @this.Height.Value,
                    Width = @this.Width.Value,
                    X = @this.X.Value,
                    Y = @this.Y.Value,
                };
            }
        }
    }
}

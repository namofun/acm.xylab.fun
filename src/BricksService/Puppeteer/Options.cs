using Newtonsoft.Json;

namespace Xylab.BricksService.Puppeteer
{
    public enum OutputType
    {
        pdf,
        html,
        screenshot,
    }

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

        public static PuppeteerSharp.ViewPortOptions Reassign(ViewPortOptions @this, PuppeteerSharp.ViewPortOptions options)
        {
            options ??= new();
            @this ??= new();
            options.Width = @this.Width;
            options.Height = @this.Height;
            options.DeviceScaleFactor = @this.DeviceScaleFactor ?? options.DeviceScaleFactor;
            options.IsMobile = @this.IsMobile ?? options.IsMobile;
            options.HasTouch = @this.HasTouch ?? options.HasTouch;
            options.IsLandscape = @this.IsLandscape ?? options.IsLandscape;
            return options;
        }
    }

    public class GotoOptions
    {
        [JsonProperty("timeout")]
        public string Timeout { get; set; }

        [JsonProperty("waitUntil")]
        public string WaitUntil { get; set; }
    }

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
    }

    public class PdfOptions
    {
        [JsonProperty("fullPage")]
        public bool FullPage { get; set; }

        [JsonProperty("scale")]
        public string Scale { get; set; }

        [JsonProperty("displayHeaderFooter")]
        public bool DisplayHeaderFooter { get; set; }

        [JsonProperty("footerTemplate")]
        public string FooterTemplate { get; set; }

        [JsonProperty("headerTemplate")]
        public bool HeaderTemplate { get; set; }

        [JsonProperty("landscape")]
        public bool Landscape { get; set; }

        [JsonProperty("pageRanges")]
        public string PageRanges { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("margin")]
        public MarginOptions Margin { get; set; }

        [JsonProperty("printBackground")]
        public bool PrintBackground { get; set; } = true;
    }

    public class ClipOptions
    {
        [JsonProperty("x")]
        public string X { get; set; }

        [JsonProperty("y")]
        public string Y { get; set; }

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }
    }

    public enum ScreenshotType
    {
        png,
        jpeg,
    }

    public class ScreenshotOptions
    {
        [JsonProperty("fullPage")]
        public bool FullPage { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("type")]
        public ScreenshotType Type { get; set; }

        [JsonProperty("clip")]
        public ClipOptions Clip { get; set; }

        [JsonProperty("selector")]
        public string Selector { get; set; }

        [JsonProperty("omitBackground")]
        public bool OmitBackground { get; set; }
    }

    public class RenderOptions
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }

        [JsonProperty("attachmentName")]
        public string AttachmentName { get; set; }

        [JsonProperty("scrollPage")]
        public bool ScrollPage { get; set; }

        [JsonProperty("emulateScreenMedia")]
        public bool EmulateScreenMedia { get; set; }

        [JsonProperty("EnableGPU")]
        public bool EnableGPU { get; set; }

        [JsonProperty("ignoreHttpsErrors")]
        public bool IgnoreHttpsErrors { get; set; }

        [JsonProperty("waitFor")]
        public object WaitFor { get; set; }

        [JsonProperty("output")]
        public OutputType Output { get; set; }

        [JsonProperty("viewport")]
        public ViewPortOptions ViewPort { get; set; }

        [JsonProperty("goto")]
        public GotoOptions Goto { get; set; }

        [JsonProperty("pdf")]
        public PdfOptions Pdf { get; set; }

        [JsonProperty("screenshot")]
        public ScreenshotOptions Screenshot { get; set; }
    }
}

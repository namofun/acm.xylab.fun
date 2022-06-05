using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Clip = PuppeteerSharp.Media.Clip;
using InteropMarginOptions = PuppeteerSharp.Media.MarginOptions;
using InteropNavigationOptions = PuppeteerSharp.NavigationOptions;
using InteropPdfOptions = PuppeteerSharp.PdfOptions;
using InteropScreenshotOptions = PuppeteerSharp.ScreenshotOptions;
using InteropViewPortOptions = PuppeteerSharp.ViewPortOptions;
using PaperFormat = PuppeteerSharp.Media.PaperFormat;
using WaitUntilNavigation = PuppeteerSharp.WaitUntilNavigation;

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

    public class PdfOptions
    {
        [JsonProperty("fullPage")]
        public bool FullPage { get; set; }

        [JsonProperty("scale")]
        public decimal Scale { get; set; } = 1;

        [JsonProperty("displayHeaderFooter")]
        public bool DisplayHeaderFooter { get; set; }

        [JsonProperty("footerTemplate")]
        public string FooterTemplate { get; set; }

        [JsonProperty("headerTemplate")]
        public string HeaderTemplate { get; set; }

        [JsonProperty("landscape")]
        public bool Landscape { get; set; }

        [JsonProperty("pageRanges")]
        public string PageRanges { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; } = "A4";

        [JsonProperty("width")]
        public string Width { get; set; }

        [JsonProperty("height")]
        public string Height { get; set; }

        [JsonProperty("margin")]
        public MarginOptions Margin { get; set; }

        [JsonProperty("printBackground")]
        public bool PrintBackground { get; set; } = true;

        [JsonProperty("omitBackground")]
        public bool OmitBackground { get; set; } = true;

        public static implicit operator InteropPdfOptions(PdfOptions pdfOptions)
        {
            PaperFormat format;
            if (pdfOptions.Format == null)
            {
                format = null;
            }
            else if (KnownPaperFormat.ContainsKey(pdfOptions.Format))
            {
                format = KnownPaperFormat[pdfOptions.Format];
            }
            else if (pdfOptions.Format.Split('x') is { Length: 2 } splitted
                && decimal.TryParse(splitted[0], out decimal width)
                && decimal.TryParse(splitted[1], out decimal height))
            {
                format = new PaperFormat(width, height);
            }
            else
            {
                throw new InvalidOperationException("Unexpected format");
            }

            return new InteropPdfOptions
            {
                DisplayHeaderFooter = pdfOptions.DisplayHeaderFooter,
                FooterTemplate = pdfOptions.FooterTemplate,
                HeaderTemplate = pdfOptions.HeaderTemplate,
                Landscape = pdfOptions.Landscape,
                PageRanges = pdfOptions.PageRanges,
                PrintBackground = pdfOptions.PrintBackground,
                Width = pdfOptions.Width,
                Height = pdfOptions.Height,
                Scale = pdfOptions.Scale,
                OmitBackground = pdfOptions.OmitBackground,
                Format = format,
                MarginOptions = pdfOptions.Margin,
            };
        }

        private static readonly IReadOnlyDictionary<string, PaperFormat> KnownPaperFormat
            = new Dictionary<string, PaperFormat>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(PaperFormat.A0)] = PaperFormat.A0,
                [nameof(PaperFormat.A1)] = PaperFormat.A1,
                [nameof(PaperFormat.A2)] = PaperFormat.A2,
                [nameof(PaperFormat.A3)] = PaperFormat.A3,
                [nameof(PaperFormat.A4)] = PaperFormat.A4,
                [nameof(PaperFormat.A5)] = PaperFormat.A5,
                [nameof(PaperFormat.A6)] = PaperFormat.A6,
                [nameof(PaperFormat.Ledger)] = PaperFormat.Ledger,
                [nameof(PaperFormat.Legal)] = PaperFormat.Legal,
                [nameof(PaperFormat.Letter)] = PaperFormat.Letter,
                [nameof(PaperFormat.Tabloid)] = PaperFormat.Tabloid,
            };
    }

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

    public enum ScreenshotType
    {
        jpeg,
        png,
    }

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

    public enum FailureHandling
    {
        all,
        page,
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

        [JsonProperty("enableGpu")]
        public bool EnableGPU { get; set; }

        [JsonProperty("ignoreHttpsErrors")]
        public bool IgnoreHttpsErrors { get; set; }

        [JsonProperty("waitFor")]
        public object WaitFor { get; set; }

        [JsonProperty("output")]
        public OutputType Output { get; set; }

        [JsonProperty("viewport")]
        public ViewPortOptions ViewPort { get; set; } = new();

        [JsonProperty("goto")]
        public GotoOptions Goto { get; set; } = new();

        [JsonProperty("pdf")]
        public PdfOptions Pdf { get; set; } = new();

        [JsonProperty("screenshot")]
        public ScreenshotOptions Screenshot { get; set; } = new();

        [JsonProperty("failEarly")]
        public FailureHandling FailEarly { get; set; }

        private static RenderOptions GetOptionsFromQuery(IQueryCollection query)
        {
            JObject root = new();
            try
            {
                foreach (var (key, values) in query)
                {
                    string value = values.First();
                    string[] addr = key.Split('.');
                    JObject holder = root;
                    for (int i = 0; i < addr.Length - 1; i++)
                    {
                        if (!holder.ContainsKey(addr[i]))
                        {
                            holder[addr[i]] = new JObject();
                        }

                        holder = (JObject)holder[addr[i]];
                    }

                    holder[addr[^1]] = value;
                }
            }
            catch (JsonException ex)
            {
                throw new BricksException(ex, "Unable to parse input query.", HttpStatusCode.BadRequest);
            }

            return root.ToObject<RenderOptions>();
        }

        private static async Task<RenderOptions> ParseRequestCore(HttpRequest request)
        {
            if (request.Query.ContainsKey("html"))
            {
                throw new BricksException("Do not support 'html' in query.", HttpStatusCode.BadRequest);
            }
            else if (HttpMethods.IsGet(request.Method))
            {
                if (!request.Query.ContainsKey("url"))
                {
                    throw new BricksException("No 'url' provided.", HttpStatusCode.BadRequest);
                }
                else
                {
                    return GetOptionsFromQuery(request.Query);
                }
            }
            else if (HttpMethods.IsPost(request.Method))
            {
                if (request.ContentType.StartsWith("application/json"))
                {
                    string body = await request.ReadAsStringAsync();
                    RenderOptions options = JsonConvert.DeserializeObject<RenderOptions>(body);

                    if (string.IsNullOrEmpty(options.Url) == string.IsNullOrEmpty(options.Html))
                    {
                        throw new BricksException("Input must contains only either URL or HTML", HttpStatusCode.BadRequest);
                    }

                    return options;
                }
                else if (request.ContentType.StartsWith("text/html"))
                {
                    if (request.Query.ContainsKey("url"))
                    {
                        throw new BricksException("Do not support 'url' for text/html content type.", HttpStatusCode.BadRequest);
                    }

                    RenderOptions options = GetOptionsFromQuery(request.Query);
                    options.Html = await request.ReadAsStringAsync();
                    return options;
                }
                else
                {
                    throw new BricksException($"Do not support content type '{request.ContentType}'.", HttpStatusCode.MethodNotAllowed);
                }
            }
            else
            {
                throw new BricksException("Unknown method.", HttpStatusCode.MethodNotAllowed);
            }
        }

        public static async Task<RenderOptions> FromRequest(HttpRequest request)
        {
            RenderOptions options = await ParseRequestCore(request);
            return options;
        }
    }

    internal struct HttpActivity
    {
        public readonly PuppeteerSharp.Request Request;
        public readonly PuppeteerSharp.Response Response;

        public HttpActivity(PuppeteerSharp.Request request)
        {
            this.Request = request;
            this.Response = null;
        }

        public HttpActivity(PuppeteerSharp.Response response)
        {
            this.Request = null;
            this.Response = response;
        }

        public HttpStatusCode Status => (this.Request?.Response ?? this.Response)?.Status ?? 0;

        public string Url => this.Request?.Url ?? this.Response?.Url;

        public static implicit operator HttpActivity(PuppeteerSharp.Request request) => new(request);
        public static implicit operator HttpActivity(PuppeteerSharp.Response response) => new(response);
    }
}

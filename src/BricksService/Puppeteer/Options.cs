using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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

        public PuppeteerSharp.ViewPortOptions Reassign()
        {
            PuppeteerSharp.ViewPortOptions options = new();
            options.Width = this.Width;
            options.Height = this.Height;
            options.DeviceScaleFactor = this.DeviceScaleFactor ?? options.DeviceScaleFactor;
            options.IsMobile = this.IsMobile ?? options.IsMobile;
            options.HasTouch = this.HasTouch ?? options.HasTouch;
            options.IsLandscape = this.IsLandscape ?? options.IsLandscape;
            return options;
        }
    }

    public class GotoOptions
    {
        [JsonProperty("timeout")]
        public int? Timeout { get; set; }

        [JsonProperty("waitUntil")]
        public string WaitUntil { get; set; } = "networkidle0";

        private static readonly Dictionary<string, PuppeteerSharp.WaitUntilNavigation> map
            = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Load"] = PuppeteerSharp.WaitUntilNavigation.Load,
                ["DOMContentLoaded"] = PuppeteerSharp.WaitUntilNavigation.DOMContentLoaded,
                ["Networkidle0"] = PuppeteerSharp.WaitUntilNavigation.Networkidle0,
                ["Networkidle2"] = PuppeteerSharp.WaitUntilNavigation.Networkidle2,
            };

        public PuppeteerSharp.NavigationOptions Reassign()
        {
            PuppeteerSharp.NavigationOptions options = new();
            options.Timeout = this.Timeout;
            options.WaitUntil = this.WaitUntil.Split(',').Select(e => map[e]).ToArray();
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
    }

    public class PdfOptions
    {
        [JsonProperty("fullPage")]
        public bool FullPage { get; set; }

        [JsonProperty("scale")]
        public double Scale { get; set; }

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
        public string Format { get; set; } = "A4";

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
        public ViewPortOptions ViewPort { get; set; } = new();

        [JsonProperty("goto")]
        public GotoOptions Goto { get; set; } = new();

        [JsonProperty("pdf")]
        public PdfOptions Pdf { get; set; } = new();

        [JsonProperty("screenshot")]
        public ScreenshotOptions Screenshot { get; set; } = new();

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
}

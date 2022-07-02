using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Xylab.BricksService.Puppeteer
{
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
                throw new ServiceException(ex, "Unable to parse input query.", HttpStatusCode.BadRequest);
            }

            return root.ToObject<RenderOptions>();
        }

        private static async Task<RenderOptions> ParseRequestCore(HttpRequest request)
        {
            if (request.Query.ContainsKey("html"))
            {
                throw new ServiceException("Do not support 'html' in query.", HttpStatusCode.BadRequest);
            }
            else if (HttpMethods.IsGet(request.Method))
            {
                if (!request.Query.ContainsKey("url"))
                {
                    throw new ServiceException("No 'url' provided.", HttpStatusCode.BadRequest);
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
                        throw new ServiceException("Input must contains only either URL or HTML", HttpStatusCode.BadRequest);
                    }

                    return options;
                }
                else if (request.ContentType.StartsWith("text/html"))
                {
                    if (request.Query.ContainsKey("url"))
                    {
                        throw new ServiceException("Do not support 'url' for text/html content type.", HttpStatusCode.BadRequest);
                    }

                    RenderOptions options = GetOptionsFromQuery(request.Query);
                    options.Html = await request.ReadAsStringAsync();
                    return options;
                }
                else
                {
                    throw new ServiceException($"Do not support content type '{request.ContentType}'.", HttpStatusCode.MethodNotAllowed);
                }
            }
            else
            {
                throw new ServiceException("Unknown method.", HttpStatusCode.MethodNotAllowed);
            }
        }

        public static async Task<RenderOptions> FromRequest(HttpRequest request)
        {
            RenderOptions options = await ParseRequestCore(request);
            return options;
        }
    }
}

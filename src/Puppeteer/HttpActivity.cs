using System.Net;

namespace Xylab.BricksService.Puppeteer
{
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

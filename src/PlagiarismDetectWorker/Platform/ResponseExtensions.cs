using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Xylab.PlagiarismDetect.Worker
{
    internal static class ResponseExtensions
    {
        public static IActionResult Respond(this HttpRequest httpRequest, object result, int? statusCode = null)
        {
            if (result == null) return new NotFoundResult();

            bool useV2 = false;
            if (httpRequest.Headers.TryGetValue("x-plag-version", out StringValues value)
                && value.Count == 1
                && value[0] == "v2")
                useV2 = true;

            if (httpRequest.Query.TryGetValue("apiVersion", out value)
                && value.Count == 1
                && value[0] == "v2")
                useV2 = true;

            JsonSerializerSettings settings = new();
            settings.ContractResolver = useV2
                ? CompatibleV2JsonContractResolver.Instance
                : CompatibleV3JsonContractResolver.Instance;

            return new JsonResult(result, settings) { StatusCode = statusCode };
        }

        public static IActionResult CreatedAt(this HttpRequest httpRequest, object result, string location)
        {
            httpRequest.HttpContext.Response.Headers.Location = location;
            return Respond(httpRequest, result, StatusCodes.Status201Created);
        }

        public static int? GetQueryInt(this HttpRequest httpRequest, string keyName)
        {
            return httpRequest.Query.TryGetValue(keyName, out StringValues vs)
                && vs.Count == 1
                && int.TryParse(vs[0], out int value)
                ? value
                : null;
        }

        public static int? GetQueryUInt(this HttpRequest httpRequest, string keyName)
        {
            return httpRequest.Query.TryGetValue(keyName, out StringValues vs)
                && vs.Count == 1
                && int.TryParse(vs[0], out int value)
                ? value >= 0 ? value : throw new ArgumentOutOfRangeException(keyName)
                : null;
        }

        public static double? GetQueryDouble(this HttpRequest httpRequest, string keyName)
        {
            return httpRequest.Query.TryGetValue(keyName, out StringValues vs)
                && vs.Count == 1
                && double.TryParse(vs[0], out double value)
                ? value
                : null;
        }

        public static string GetQueryString(this HttpRequest httpRequest, string keyName)
        {
            return httpRequest.Query.TryGetValue(keyName, out StringValues vs)
                && vs.Count == 1
                ? vs[0]
                : null;
        }

        public static string GetQuerySwitch(this HttpRequest httpRequest, string keyName, string[] validValues, string defaultValue)
        {
            return httpRequest.Query.TryGetValue(keyName, out StringValues vs)
                && vs.Count == 1
                && validValues.Contains(vs[0])
                ? vs[0]
                : defaultValue;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Microsoft.Azure.Functions
{
    public static class FunctionsHttpHelper
    {
        public static bool IsAuthorized(this HttpRequest req)
        {
            if (req.HttpContext.Items.TryGetValue(FunctionAuthorizeAttribute.Key, out var faa)
                && faa is FunctionAuthorizeAttribute authorizeAttribute)
            {
                return authorizeAttribute.IsAuthorized();
            }
            else
            {
                return true;
            }
        }

        public static StatusCodeResult Ok(this HttpRequest req)
        {
            return new StatusCodeResult(StatusCodes.Status200OK);
        }

        public static StatusCodeResult Forbid(this HttpRequest req)
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        public static StatusCodeResult NotFound(this HttpRequest req)
        {
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }

        public static StatusCodeResult BadRequest(this HttpRequest req)
        {
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        }

        public static StatusCodeResult ServiceUnavailable(this HttpRequest req)
        {
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
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

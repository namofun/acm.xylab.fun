using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xylab.PlagiarismDetect.Worker
{
    internal static class ResponseExtensions
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

        public static async Task<ActionResult<TResult>> Execute<TResult>(
            this HttpRequest req,
            Func<Task<TResult>> action)
        {
            if (!req.IsAuthorized())
            {
                return req.Forbid();
            }

            try
            {
                return await action();
            }
            catch (ArgumentOutOfRangeException)
            {
                return req.BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return req.NotFound();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                return req.ServiceUnavailable();
            }
        }

        public static Task<ActionResult<TResult>> Execute<TResult, TParam1>(
            this HttpRequest req,
            Func<TParam1, Task<TResult>> action,
            TParam1 param1)
            => Execute(req, () => action(param1));

        public static Task<ActionResult<TResult>> Execute<TResult, TParam1, TParam2>(
            this HttpRequest req,
            Func<TParam1, TParam2, Task<TResult>> action,
            TParam1 param1, TParam2 param2)
            => Execute(req, () => action(param1, param2));

        public static Task<ActionResult<TResult>> Execute<TResult, TParam1, TParam2, TParam3>(
            this HttpRequest req,
            Func<TParam1, TParam2, TParam3, Task<TResult>> action,
            TParam1 param1, TParam2 param2, TParam3 param3)
            => Execute(req, () => action(param1, param2, param3));

        public static Task<ActionResult<TResult>> Execute<TResult, TParam1, TParam2, TParam3, TParam4, TParam5>(
            this HttpRequest req,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<TResult>> action,
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5)
            => Execute(req, () => action(param1, param2, param3, param4, param5));

        public static Task<ActionResult<TResult>> Execute<TResult, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(
            this HttpRequest req,
            Func<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, Task<TResult>> action,
            TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4, TParam5 param5, TParam6 param6, TParam7 param7, TParam8 param8, TParam9 param9)
            => Execute(req, () => action(param1, param2, param3, param4, param5, param6, param7, param8, param9));
    }
}

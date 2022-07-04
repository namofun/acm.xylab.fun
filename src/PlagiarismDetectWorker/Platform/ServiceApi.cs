using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Worker
{
    [FunctionAuthorize("PlagiarismDetectSystem.All")]
    public class ServiceApi
    {
        private readonly IPlagiarismDetectService _service;
        private readonly InjectSignalProvider _signalProvider;

        public ServiceApi(
            IPlagiarismDetectService service,
            InjectSignalProvider signalProvider)
            => (_service, _signalProvider) = (service, signalProvider);

        [FunctionName("PlatformGetAllLanguages")]
        public Task<IActionResult> GetAllLanguages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/languages")] HttpRequest req)
            => WrapExceptions(async () => req.Respond(await _service.ListLanguageAsync()));

        [FunctionName("PlatformGetOneLanguage")]
        public Task<IActionResult> GetOneLanguage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/languages/{id}")] HttpRequest req,
            string id)
            => WrapExceptions(async () => req.Respond(await _service.FindLanguageAsync(id)));

        [FunctionName("PlatformGetOneReport")]
        public Task<IActionResult> GetOneReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/reports/{id}")] HttpRequest req,
            string id)
            => WrapExceptions(async () => req.Respond(await _service.FindReportAsync(id)));

        [FunctionName("PlatformGetAllSets")]
        public Task<IActionResult> GetAllSets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets")] HttpRequest req)
            => WrapExceptions(async ()
                => req.Respond(
                    await _service.ListSetsAsync(
                        req.GetQueryInt("related"),
                        req.GetQueryInt("creator"),
                        req.GetQueryUInt("skip"),
                        req.GetQueryUInt("limit"),
                        req.GetQuerySwitch("order", new[] { "asc", "desc" }, "desc") == "asc")));

        [FunctionName("PlatformGetOneSet")]
        public Task<IActionResult> GetOneSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}")] HttpRequest req,
            string sid)
            => WrapExceptions(async () => req.Respond(await _service.FindSetAsync(sid)));

        [FunctionName("PlatformGetAllSubmissions")]
        public Task<IActionResult> GetAllSubmissions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions")] HttpRequest req,
            string sid)
            => WrapExceptions(async ()
                => req.Respond(
                    await _service.ListSubmissionsAsync(
                        sid,
                        req.GetQueryString("language"),
                        req.GetQueryInt("exclusive_category"),
                        req.GetQueryInt("inclusive_category"),
                        req.GetQueryDouble("min_percent"),
                        req.GetQueryUInt("skip"),
                        req.GetQueryUInt("limit"),
                        req.GetQuerySwitch("by", new[] { "id", "percent" }, "id"),
                        req.GetQuerySwitch("order", new[] { "asc", "desc" }, "asc") == "asc")));

        [FunctionName("PlatformGetOneSubmission")]
        public Task<IActionResult> GetOneSubmission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}")] HttpRequest req,
            string sid,
            int id)
            => WrapExceptions(async ()
                => req.Respond(
                    await _service.FindSubmissionAsync(
                        sid,
                        id,
                        !req.Query.TryGetValue("includeFiles", out var values)
                            || values.Count != 1
                            || !bool.TryParse(values[0], out bool result)
                            || result)));

        [FunctionName("PlatformGetOneSubmissionFile")]
        public Task<IActionResult> GetOneSubmissionFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/files")] HttpRequest req,
            string sid,
            int id)
            => WrapExceptions(async () => req.Respond(await _service.GetFilesAsync(sid, id)));

        [FunctionName("PlatformGetOneSubmissionCompilation")]
        public Task<IActionResult> GetOneSubmissionCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/compilation")] HttpRequest req,
            string sid,
            int id)
            => WrapExceptions(async () => req.Respond(await _service.GetCompilationAsync(sid, id)));

        [FunctionName("PlatformGetOneSubmissionComparisons")]
        public Task<IActionResult> GetOneSubmissionComparisons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/comparisons")] HttpRequest req,
            string sid,
            int id)
            => WrapExceptions(async ()
                => req.Respond(
                    await _service.GetComparisonsBySubmissionAsync(
                        sid,
                        id,
                        req.Query.TryGetValue("includeFiles", out var values)
                            && values.Count == 1
                            && bool.TryParse(values[0], out bool result)
                            && result)));

        [FunctionName("PlatformDeleteOneSubmissionCompilation")]
        public Task<IActionResult> DeleteOneSubmissionCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "plagiarism/sets/{sid}/submissions/{id}/compilation")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            string sid,
            int id)
            => WrapExceptions(async () =>
            {
                _signalProvider.CompileSignal = submissionTokenizer;
                await _service.ResetCompilationAsync(sid, id);
                return req.Respond(_service.GetVersion());
            });

        [FunctionName("PlatformRescue")]
        public Task<IActionResult> Rescue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/rescue")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator)
            => WrapExceptions(async () =>
            {
                _signalProvider.ReportSignal = reportGenerator;
                _signalProvider.CompileSignal = submissionTokenizer;
                await _service.RescueAsync();
                return req.Respond(_service.GetVersion());
            });

        [FunctionName("PlatformPatchOneReport")]
        public Task<IActionResult> PatchOneReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "plagiarism/reports/{id}")] HttpRequest req,
            string id)
            => WrapExceptions(async () =>
            {
                if (!req.HasFormContentType) return new BadRequestResult();
                Dictionary<string, string> updatedProps = new();

                if (req.Form.TryGetValue("justification", out var justifications))
                {
                    if (justifications.Count != 1)
                    {
                        return new BadRequestResult();
                    }
                    else if (string.Equals(justifications[0], "null", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(justifications[0], "Unspecified", StringComparison.OrdinalIgnoreCase))
                    {
                        await _service.JustificateAsync(id, ReportJustification.Unspecified);
                    }
                    else if (string.Equals(justifications[0], "true", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(justifications[0], "Claimed", StringComparison.OrdinalIgnoreCase))
                    {
                        await _service.JustificateAsync(id, ReportJustification.Claimed);
                    }
                    else if (string.Equals(justifications[0], "false", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(justifications[0], "Ignored", StringComparison.OrdinalIgnoreCase))
                    {
                        await _service.JustificateAsync(id, ReportJustification.Ignored);
                    }
                    else
                    {
                        return new BadRequestResult();
                    }

                    updatedProps.Add("justification", justifications[0].ToLower());
                }

                if (req.Form.TryGetValue("shared", out var shareds))
                {
                    if (shareds.Count != 1)
                    {
                        return new BadRequestResult();
                    }
                    else if (string.Equals(shareds[0], "true", StringComparison.OrdinalIgnoreCase))
                    {
                        await _service.ShareReportAsync(id, true);
                    }
                    else if (string.Equals(shareds[0], "false", StringComparison.OrdinalIgnoreCase))
                    {
                        await _service.ShareReportAsync(id, false);
                    }
                    else
                    {
                        return new BadRequestResult();
                    }

                    updatedProps.Add("shared", shareds[0].ToLower());
                }

                return new OkObjectResult(updatedProps);
            });

        [FunctionName("PlatformCreateOneSet")]
        public Task<IActionResult> CreateOneSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/sets")] HttpRequest req)
            => WrapExceptions(async () =>
            {
                if (!req.HasJsonContentType()) return new BadRequestResult();
                SetCreation set = await req.ReadFromJsonAsync<SetCreation>();
                if (string.IsNullOrEmpty(set.Name)) return new BadRequestResult();
                PlagiarismSet setEntity = await _service.CreateSetAsync(set);
                return req.CreatedAt(setEntity, "/api/plagiarism/sets/" + setEntity.Id);
            });

        [FunctionName("PlatformCreateOneSubmission")]
        public Task<IActionResult> CreateOneSubmission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/sets/{sid}/submissions")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            string sid)
            => WrapExceptions(async () =>
            {
                _signalProvider.CompileSignal = submissionTokenizer;

                if (!req.HasJsonContentType()) return new BadRequestResult();
                SubmissionCreation submission = await req.ReadFromJsonAsync<SubmissionCreation>();
                if (submission.SetId != sid) return new BadRequestResult();
                if (submission.Files == null || submission.Files.Count == 0) return new BadRequestResult();
                var s = await _service.SubmitAsync(submission);

                if (s.Files == null)
                {
                    s.Files = submission.Files
                        .Select((s, j) => new SubmissionFile
                        {
                            FileId = j + 1,
                            FileName = s.FileName,
                            FilePath = s.FilePath,
                        })
                        .ToList();
                }

                return req.CreatedAt(s, "/api/plagiarism/sets/" + s.SetId + "/submissions/" + s.Id);
            });

        private static async Task<IActionResult> WrapExceptions(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (ArgumentOutOfRangeException)
            {
                return new BadRequestResult();
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}

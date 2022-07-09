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
        {
            _service = service;
            _signalProvider = signalProvider;
        }

        [FunctionName("PlatformGetAllLanguages")]
        public Task<ActionResult<List<LanguageInfo>>> GetAllLanguages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/languages")] HttpRequest req)
        {
            return req.Execute(_service.ListLanguageAsync);
        }

        [FunctionName("PlatformGetOneLanguage")]
        public Task<ActionResult<LanguageInfo>> GetOneLanguage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/languages/{id}")] HttpRequest req,
            string id)
        {
            return req.Execute(_service.FindLanguageAsync, id);
        }

        [FunctionName("PlatformGetOneReport")]
        public Task<ActionResult<Backend.Models.Report>> GetOneReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/reports/{id}")] HttpRequest req,
            string id)
        {
            return req.Execute(_service.FindReportAsync, id);
        }

        [FunctionName("PlatformGetAllSets")]
        public Task<ActionResult<IReadOnlyList<PlagiarismSet>>> GetAllSets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets")] HttpRequest req)
        {
            return req.Execute(
                _service.ListSetsAsync,
                    req.GetQueryInt("related"),
                    req.GetQueryInt("creator"),
                    req.GetQueryUInt("skip"),
                    req.GetQueryUInt("limit"),
                    req.GetQuerySwitch("order", new[] { "asc", "desc" }, "desc") == "asc");
        }

        [FunctionName("PlatformGetOneSet")]
        public Task<ActionResult<PlagiarismSet>> GetOneSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}")] HttpRequest req,
            string sid)
        {
            return req.Execute(_service.FindSetAsync, sid);
        }

        [FunctionName("PlatformGetAllSubmissions")]
        public Task<ActionResult<IReadOnlyList<Submission>>> GetAllSubmissions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions")] HttpRequest req,
            string sid)
        {
            return req.Execute(
                _service.ListSubmissionsAsync,
                    sid,
                    req.GetQueryString("language"),
                    req.GetQueryInt("exclusive_category"),
                    req.GetQueryInt("inclusive_category"),
                    req.GetQueryDouble("min_percent"),
                    req.GetQueryUInt("skip"),
                    req.GetQueryUInt("limit"),
                    req.GetQuerySwitch("by", new[] { "id", "percent" }, "id"),
                    req.GetQuerySwitch("order", new[] { "asc", "desc" }, "asc") == "asc");
        }

        [FunctionName("PlatformGetOneSubmission")]
        public Task<ActionResult<Submission>> GetOneSubmission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}")] HttpRequest req,
            string sid, int id)
        {
            return req.Execute(
                _service.FindSubmissionAsync,
                    sid,
                    id,
                    !req.Query.TryGetValue("includeFiles", out var values)
                        || values.Count != 1
                        || !bool.TryParse(values[0], out bool result)
                        || result);
        }

        [FunctionName("PlatformGetOneSubmissionFile")]
        public Task<ActionResult<IReadOnlyList<SubmissionFile>>> GetOneSubmissionFile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/files")] HttpRequest req,
            string sid, int id)
        {
            return req.Execute(_service.GetFilesAsync, sid, id);
        }

        [FunctionName("PlatformGetOneSubmissionCompilation")]
        public Task<ActionResult<Compilation>> GetOneSubmissionCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/compilation")] HttpRequest req,
            string sid, int id)
        {
            return req.Execute(_service.GetCompilationAsync, sid, id);
        }

        [FunctionName("PlatformGetOneSubmissionComparisons")]
        public Task<ActionResult<Vertex>> GetOneSubmissionComparisons(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "plagiarism/sets/{sid}/submissions/{id}/comparisons")] HttpRequest req,
            string sid, int id)
        {
            return req.Execute(
                _service.GetComparisonsBySubmissionAsync,
                    sid,
                    id,
                    req.Query.TryGetValue("includeFiles", out var values)
                        && values.Count == 1
                        && bool.TryParse(values[0], out bool result)
                        && result);
        }

        [FunctionName("PlatformDeleteOneSubmissionCompilation")]
        public async Task<ActionResult<ServiceVersion>> DeleteOneSubmissionCompilation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "plagiarism/sets/{sid}/submissions/{id}/compilation")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            string sid, int id)
        {
            if (!req.IsAuthorized()) return req.Forbid();

            _signalProvider.CompileSignal = submissionTokenizer;
            await _service.ResetCompilationAsync(sid, id);

            return _service.GetVersion();
        }

        [FunctionName("PlatformRescue")]
        public async Task<ActionResult<ServiceVersion>> Rescue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/rescue")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            [Queue(Startup.ReportGeneratingQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> reportGenerator)
        {
            if (!req.IsAuthorized()) return req.Forbid();

            _signalProvider.ReportSignal = reportGenerator;
            _signalProvider.CompileSignal = submissionTokenizer;
            await _service.RescueAsync();

            return _service.GetVersion();
        }

        [FunctionName("PlatformPatchOneReport")]
        public async Task<ActionResult<Dictionary<string, string>>> PatchOneReport(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "plagiarism/reports/{id}")] HttpRequest req,
            string id)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            if (!req.HasFormContentType) return req.BadRequest();
            if (null == await _service.FindReportAsync(id)) return req.NotFound();
            Dictionary<string, string> updatedProps = new();

            if (req.Form.TryGetValue("justification", out var justifications))
            {
                if (justifications.Count != 1
                    || !Enum.TryParse(justifications[0], out ReportJustification justification))
                    return req.BadRequest();

                await _service.JustificateAsync(id, justification);
                updatedProps.Add("justification", justification.ToString().ToLower());
            }

            if (req.Form.TryGetValue("shared", out var shareds))
            {
                if (shareds.Count != 1
                    || !bool.TryParse(shareds[0], out bool shared))
                    return req.BadRequest();

                await _service.ShareReportAsync(id, shared);
                updatedProps.Add("shared", shared.ToString().ToLower());
            }

            return updatedProps;
        }

        [FunctionName("PlatformCreateOneSet")]
        public async Task<ActionResult<PlagiarismSet>> CreateOneSet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/sets")] HttpRequest req)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            if (!req.HasJsonContentType()) return req.BadRequest();

            SetCreation set = await req.ReadFromJsonAsync<SetCreation>();
            if (string.IsNullOrEmpty(set.Name)) return req.BadRequest();
            PlagiarismSet setEntity = await _service.CreateSetAsync(set);

            return new CreatedResult("/api/plagiarism/sets/" + setEntity.Id, setEntity);
        }

        [FunctionName("PlatformCreateOneSubmission")]
        public async Task<ActionResult<Submission>> CreateOneSubmission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "plagiarism/sets/{sid}/submissions")] HttpRequest req,
            [Queue(Startup.CompilationQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> submissionTokenizer,
            string sid)
        {
            if (!req.IsAuthorized()) return req.Forbid();
            if (!req.HasJsonContentType()) return req.BadRequest();

            SubmissionCreation submission = await req.ReadFromJsonAsync<SubmissionCreation>();
            if (submission.SetId != sid) return req.BadRequest();
            if (submission.Files == null || submission.Files.Count == 0) return req.BadRequest();

            _signalProvider.CompileSignal = submissionTokenizer;
            Submission s;
            try
            {
                s = await _service.SubmitAsync(submission);
            }
            catch (KeyNotFoundException)
            {
                return req.NotFound();
            }

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

            return new CreatedResult("/api/plagiarism/sets/" + s.SetId + "/submissions/" + s.Id, s);
        }
    }
}

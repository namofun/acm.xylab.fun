using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ccs.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Polygon.Storages;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class AzureStorageAccount : IProblemFileProvider, IContestFileProvider, IJudgingFileProvider
    {
        private readonly AzureBlobProvider _blobProvider;

        public AzureStorageAccount(
            BlobContainerClient client,
            string localFileCachePath,
            bool allowAutoCache = false,
            AccessTier? defaultAccessTier = null)
        {
            _blobProvider = new AzureBlobProvider(
                client,
                localFileCachePath,
                defaultAccessTier,
                allowAutoCache);

            ContainerClient = client;
        }

        public BlobContainerClient ContainerClient { get; }

        public static string JudgingOutputNameFormat(int judgingId, int runId, string type)
            => $"judgings/j{judgingId}/r{runId}.{type}";

        public static string TestcaseNameFormat(int problemId, int testcaseId, string target)
            => $"problems/p{problemId}/t{testcaseId}.{target}";

        public static string StatementNameFormat(int problemId)
            => $"problems/p{problemId}/view.html";

        public static string StatementSectionNameFormat(int problemId, string section)
            => $"problems/p{problemId}/{section}.md";

        public static string ContestReadmeSourceFormat(int contestId)
            => $"contests/c{contestId}/readme.md";

        public static string ContestReadmeNameFormat(int contestId)
            => $"contests/c{contestId}/readme.html";

        public Task<IBlobInfo> GetReadmeAsync(int contestId)
            => _blobProvider.GetFileInfoAsync(
                ContestReadmeNameFormat(contestId));

        public Task<IBlobInfo> GetReadmeSourceAsync(int contestId)
            => _blobProvider.GetFileInfoAsync(
                ContestReadmeSourceFormat(contestId));

        public Task<IBlobInfo> WriteReadmeAsync(int contestId, string content)
            => _blobProvider.WriteStringAsync(
                ContestReadmeNameFormat(contestId),
                content,
                "text/html");

        public Task<IBlobInfo> WriteReadmeSourceAsync(int contestId, string content)
            => _blobProvider.WriteStringAsync(
                ContestReadmeSourceFormat(contestId),
                content,
                "text/markdown");

        public Task<IBlobInfo> GetTestcaseFileAsync(int problemId, int testcaseId, string target)
            => _blobProvider.GetFileInfoAsync(
                TestcaseNameFormat(problemId, testcaseId, target));

        public Task<IBlobInfo> WriteTestcaseFileAsync(int problemId, int testcaseId, string target, Stream source)
            => _blobProvider.WriteStreamAsync(
                TestcaseNameFormat(problemId, testcaseId, target),
                source,
                "application/octet-stream");

        public Task<IBlobInfo> GetStatementAsync(int problemId)
            => _blobProvider.GetFileInfoAsync(
                StatementNameFormat(problemId));

        public Task<IBlobInfo> WriteStatementAsync(int problemId, string content)
            => _blobProvider.WriteStringAsync(
                StatementNameFormat(problemId),
                content,
                "text/html");

        public Task<IBlobInfo> GetStatementSectionAsync(int problemId, string section)
            => _blobProvider.GetFileInfoAsync(
                StatementSectionNameFormat(problemId, section));

        public Task<IBlobInfo> WriteStatementSectionAsync(int problemId, string section, string content)
            => _blobProvider.WriteStringAsync(
                StatementSectionNameFormat(problemId, section),
                content,
                "text/markdown");

        public Task<IBlobInfo> GetJudgingRunOutputAsync(int judgingId, int runId, string type)
            => _blobProvider.GetFileInfoAsync(
                JudgingOutputNameFormat(judgingId, runId, type));

        public Task<IBlobInfo> WriteJudgingRunOutputAsync(int judgingId, int runId, string type, byte[] content)
            => _blobProvider.WriteBinaryAsync(
                JudgingOutputNameFormat(judgingId, runId, type),
                content,
                "application/octet-stream");
    }
}

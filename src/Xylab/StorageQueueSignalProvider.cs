using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;

namespace SatelliteSite
{
    public class StorageQueueSignalProvider : ISignalProvider
    {
        private const string CompilationQueue = "pds-compilation-queue";
        private const string ReportGeneratingQueue = "pds-report-generating-queue";
        private readonly QueueServiceClient _connection;

        public StorageQueueSignalProvider(ConnectionCache connection)
        {
            _connection = connection.QueueService;
        }

        private async Task<string> SendAsync(string queueName, string intention)
        {
            Guid recordId = Guid.NewGuid();
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            string record = $"{intention}|{timestamp}|{recordId}";
            await _connection.GetQueueClient(queueName).SendMessageAsync(record);
            return record;
        }

        public Task SendCompileSignalAsync()
        {
            return SendAsync(CompilationQueue, "compile");
        }

        public Task SendReportSignalAsync()
        {
            return SendAsync(ReportGeneratingQueue, "report");
        }

        public async Task SendRescueSignalAsync()
        {
            await SendAsync(CompilationQueue, "rescue");
            await SendAsync(ReportGeneratingQueue, "rescue");
        }
    }
}

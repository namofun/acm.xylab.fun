using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;
using Xylab.PlagiarismDetect.Backend.Services;

namespace Xylab.PlagiarismDetect.Worker
{
    public class InjectSignalProvider : ISignalProvider
    {
        public IAsyncCollector<string> CompileSignal { get; set; }

        public IAsyncCollector<string> ReportSignal { get; set; }

        public async Task SendCompileSignalAsync()
        {
            if (CompileSignal != null)
            {
                Guid rescueId = Guid.NewGuid();
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                await CompileSignal.AddAsync($"ingest|{timestamp}|{rescueId}");
                await CompileSignal.FlushAsync();
            }
        }

        public async Task SendReportSignalAsync()
        {
            if (CompileSignal != null)
            {
                Guid rescueId = Guid.NewGuid();
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                await ReportSignal.AddAsync($"ingest|{timestamp}|{rescueId}");
                await ReportSignal.FlushAsync();
            }
        }

        public async Task SendRescueSignalAsync()
        {
            await SendCompileSignalAsync();
            await SendReportSignalAsync();
        }
    }
}

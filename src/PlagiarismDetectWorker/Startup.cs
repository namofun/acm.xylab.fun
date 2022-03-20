using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics;
using Xylab.PlagiarismDetect.Backend;
using Xylab.PlagiarismDetect.Backend.Services;

[assembly: FunctionsStartup(typeof(Xylab.PlagiarismDetect.Worker.Startup))]

namespace Xylab.PlagiarismDetect.Worker
{
    internal class Startup : FunctionsStartup
    {
        public const string CompilationQueue = "pds-compilation-queue";

        public const string ReportGeneratingQueue = "pds-report-generating-queue";

        public const string GraphRecoveryQueue = "pds-graph-recovery-queue";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddPlagGenerationService();
            builder.Services.AddScoped(sp => (IPlagiarismDetectService)sp.GetRequiredService<IJobContext>());

            IConfiguration configuration = builder.GetContext().Configuration;
            builder.Services.AddCosmosForPlagWorker(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("CosmosDbAccount");
                options.DatabaseName = configuration.GetConnectionString("CosmosDbName");
            });

            builder.Services.ReplaceSingleton<ISignalProvider, InjectSignalProvider>();
            builder.Services.AddSingletonDowncast<InjectSignalProvider, ISignalProvider>();
            builder.Services.AddSingleton<ITelemetryClient, FunctionsTelemetryClient>();
        }
    }
}

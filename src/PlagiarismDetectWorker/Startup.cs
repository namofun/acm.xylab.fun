using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Plag.Backend;
using Plag.Backend.Services;

[assembly: FunctionsStartup(typeof(Xylab.PlagiarismDetect.Worker.Startup))]

namespace Xylab.PlagiarismDetect.Worker
{
    internal class Startup : FunctionsStartup
    {
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
        }
    }
}

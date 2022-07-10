using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xylab.BricksService.OjUpdate;

[assembly: FunctionsStartup(typeof(Xylab.BricksService.Startup))]

namespace Xylab.BricksService
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            builder.Services.Configure<RecordV2Options>(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("CosmosDbAccount");
                options.DatabaseName = configuration.GetConnectionString("CosmosDbName");
            });

            builder.Services.AddSingleton<RecordV2Storage>();
        }
    }
}

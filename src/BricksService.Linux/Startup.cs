using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics;
using System.Text;

[assembly: FunctionsStartup(typeof(Xylab.BricksService.Startup))]

namespace Xylab.BricksService
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ITelemetryClient, FunctionsTelemetryClient>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}

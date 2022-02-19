using Azure.Core;
using Azure.Identity;
using Ccs.Registration;
using Markdig;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.MigratePolygonV1();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddApplicationInsights()
                .AddModule<IdentityModule.IdentityModule<XylabUser, Role, DefaultContext>>()
                .EnableIdentityModuleBasicAuthentication()
                .AddModule<XylabModule.XylabModule>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<XylabUser, Role, DefaultContext>>()
                .AddModule<OjUpdateModule.OjUpdateModule<DefaultContext>>()
                .AddModule<NewsModule.NewsModule<DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<ContestModule.ContestModule<Ccs.RelationalRole<XylabUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<Plag.Backend.RestfulBackendRole>>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>()
                .ConfigureServices((context, services) =>
                {
                    services.AddMarkdown();

                    services.AddDbModelSupplier<DefaultContext, Polygon.Storages.PolygonIdentityEntityConfiguration<XylabUser, DefaultContext>>();

                    services.ConfigurePolygonStorage(options =>
                    {
                        options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                        options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                    });

                    services.AddContestRegistrationTenant();

                    services.AddSingleton<ITelemetryInitializer, CloudRoleInitializer>();

                    services.Configure<ContestModule.Routing.MinimalSiteOptions>(options =>
                    {
                        options.Keyword = context.Configuration.GetConnectionString("ContestKeyword");
                    });

                    services.Configure<ContestModule.Components.ContestStatistics.ContestStatisticsOptions>(options =>
                    {
                        options.DefaultContest = context.Configuration.GetValue<int?>("DefaultProblemset");
                    });

                    services.AddSingleton<TokenCredential>(new DefaultAzureCredential());
                    services.AddTransient<AzureAppAuthHttpMessageHandler>();
                    services.AddHttpClient<Plag.Backend.Services.RestfulClient>()
                        .AddHttpMessageHandler<AzureAppAuthHttpMessageHandler>();
                });

        private class CloudRoleInitializer : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                telemetry.Context.Cloud.RoleName = "acm.xylab.fun";

                if (telemetry is DependencyTelemetry dependencyTelemetry)
                {
                    // RemoteDependencyConstants.HTTP
                    if (dependencyTelemetry.Type == "Http"
                        && dependencyTelemetry.Target.StartsWith("pds.xylab.fun:"))
                    {
                        // Remove the port to get better correlation
                        dependencyTelemetry.Target = "pds.xylab.fun";
                    }
                }
            }
        }

        private class AzureAppAuthHttpMessageHandler : DelegatingHandler
        {
            private readonly TokenCredential _tokenProvider;
            private readonly string _apiResource, _tenantId;

            public AzureAppAuthHttpMessageHandler(
                IConfiguration configuration,
                TokenCredential tokenProvider)
            {
                _tokenProvider = tokenProvider;
                _apiResource = configuration.GetConnectionString("PlagiarismApiResource");
                _tenantId = configuration.GetConnectionString("PlagiarismApiTenant");
            }

            protected async override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                if (!string.IsNullOrEmpty(_apiResource))
                {
                    AccessToken authResult =
                        await _tokenProvider.GetTokenAsync(
                            new TokenRequestContext(new[] { _apiResource }, tenantId: _tenantId),
                            cancellationToken);

                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        authResult.Token);
                }

                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}

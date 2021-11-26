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
using SatelliteSite.OjUpdateModule.Services;
using SatelliteSite.Services;
using System.IO;

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
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddModule<IdentityModule.IdentityModule<XylabUser, Role, DefaultContext>>()
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

                    services.Configure<ApplicationInsightsDisplayOptions>(options =>
                    {
                        options.ApiKey = context.Configuration["ApplicationInsights:ForDisplayAppKey"];
                        options.ApplicationId = context.Configuration["ApplicationInsights:ForDisplayAppId"];
                    });

                    services.AddSingleton<ITelemetryInitializer, CloudRoleInitializer>();

                    services.Configure<ContestModule.Routing.MinimalSiteOptions>(options =>
                    {
                        options.Keyword = context.Configuration.GetConnectionString("ContestKeyword");
                    });

                    services.Configure<ContestModule.Components.ContestStatistics.ContestStatisticsOptions>(options =>
                    {
                        options.DefaultContest = context.Configuration.GetValue<int?>("DefaultProblemset");
                    });

                    for (int i = 0; i < services.Count; i++)
                    {
                        if (services[i].ServiceType == typeof(IHostedService)
                            && services[i].ImplementationType == typeof(CfUpdateService)
                            && services[i].Lifetime == ServiceLifetime.Singleton)
                        {
                            services[i] = ServiceDescriptor.Singleton<IHostedService, CfUpdateServiceV2>();
                        }
                    }
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
    }
}

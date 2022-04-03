using Ccs.Registration;
using Markdig;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Mailing;
using SatelliteSite;
using SatelliteSite.XylabModule.Services;
using System;
using System.Linq;
using Xylab.BricksService.OjUpdate;

[assembly: RoleDefinition(17, "TeamLeader", "leader", "Team Leader")]
[assembly: ConfigurationItem(0, "Tenant", "oj_Codeforces_update_time", typeof(DateTimeOffset?), null!, "Last update time of Codeforces.", IsPublic = false)]
[assembly: ConfigurationItem(1, "Tenant", "oj_Vjudge_update_time", typeof(DateTimeOffset?), null!, "Last update time of Vjudge.", IsPublic = false)]
[assembly: ConfigurationItem(2, "Tenant", "oj_Hdoj_update_time", typeof(DateTimeOffset?), null!, "Last update time of HDOJ.", IsPublic = false)]
// [assembly: ConfigurationItem(3, "Tenant", "oj_Poj_update_time", typeof(DateTimeOffset?), null!, "Last update time of POJ.", IsPublic = false)]

namespace SatelliteSite.XylabModule
{
    public class XylabModule : AbstractModule, IAuthorizationPolicyRegistry
    {
        public override string Area => "Xylab";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureApplicationBuilder(options =>
            {
                options.SiteName = "Online Judge";
                options.PointBeforeUrlRewriting.Insert(0, app => app.UseMiddleware<AliyunCdnRealIpMiddleware>());
            });

            services.ConfigureIdentityAdvanced(options =>
            {
                options.ShortenedClaimName = true;
            });

            services.AddMarkdown();
            services.AddContestRegistrationTenant();

            services.Remove(services.Where(sd => sd.ServiceType == typeof(IEmailSender)).Single());
            services.AddSingleton<ITelemetryInitializer, LogicAppsInitializer>();
            services.AddHttpClient<IEmailSender, LogicAppsEmailSender>()
                .AddAzureAuthHandler(new[] { "https://logic.azure.com/.default" });

            var length = configuration.GetValue<int>("OjUpdateServiceSleepLength");
            if (length < 60) length = 24 * 7 * 60;

            OjUpdateService.SleepLength = length;
            services.AddSingleton<IRecordStorage, RecordV2Storage>();
            services.AddHostedService<CfUpdateService>();
            services.AddHostedService<VjUpdateService>();
            services.AddHostedService<HdojUpdateService>();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.MainNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasTitle("fas fa-home", "Home")
                    .HasLink("/")
                    .ActiveWhenViewData("HomePage");

                menu.HasEntry(290)
                    .HasTitle("fas fa-book-open", "Problemsets")
                    .HasLink("Xylab", "Home", "Problemsets")
                    .ActiveWhenViewData("Problemset");

                menu.HasEntry(300)
                    .HasTitle("fas fa-trophy", "Contests")
                    .HasLink("Xylab", "Home", "Contests")
                    .ActiveWhenViewData("ListContest");

                menu.HasEntry(310)
                    .HasTitle("fas fa-rocket", "Gyms")
                    .HasLink("Xylab", "Home", "Gyms")
                    .ActiveWhenViewData("ListGym");
            });

            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(300)
                    .HasLink("Dashboard", "ExternalRanklist", "List")
                    .HasTitle(string.Empty, "External OJ Ranklist")
                    .RequireRoles("Administrator,TeamLeader");
            });
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.WithErrorHandler("Xylab", "Home")
                .MapStatusCode("/{**slug}");
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            container.AddPolicy2("ExternalRanklistReader",
                b => b.AcceptClaim("tenant", "10183")
                      .AcceptRole("Student"));
        }
    }
}

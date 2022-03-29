using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite;
using SatelliteSite.OjUpdateModule.Services;
using System;

[assembly: RoleDefinition(17, "TeamLeader", "leader", "Team Leader")]
[assembly: ConfigurationItem(0, "Tenant", "oj_Codeforces_update_time", typeof(DateTimeOffset?), null!, "Last update time of Codeforces.", IsPublic = false)]
[assembly: ConfigurationItem(1, "Tenant", "oj_Vjudge_update_time", typeof(DateTimeOffset?), null!, "Last update time of Vjudge.", IsPublic = false)]
[assembly: ConfigurationItem(2, "Tenant", "oj_Hdoj_update_time", typeof(DateTimeOffset?), null!, "Last update time of HDOJ.", IsPublic = false)]
// [assembly: ConfigurationItem(3, "Tenant", "oj_Poj_update_time", typeof(DateTimeOffset?), null!, "Last update time of POJ.", IsPublic = false)]

namespace SatelliteSite.OjUpdateModule
{
    public class OjUpdateModule<TContext> : AbstractModule
        where TContext : DbContext
    {
        public override string Area => "Tenant";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            var length = configuration.GetValue<int>("OjUpdateServiceSleepLength");
            if (length < 60) length = 24 * 7 * 60;

            OjUpdateService.SleepLength = length;
            services.AddScoped<IRecordStorage, RecordV1Storage<TContext>>();
            services.AddHostedService<CfUpdateService>();
            services.AddHostedService<VjUpdateService>();
            services.AddHostedService<HdojUpdateService>();
            services.AddDbModelSupplier<TContext, RecordV1EntityConfiguration<TContext>>();

            services.Configure<AuthorizationOptions>(options =>
            {
                if (options.GetPolicy("ExternalRanklistReader") == null)
                {
                    options.AddPolicy("ExternalRanklistReader", b => b.RequireAssertion(_ => true));
                }
            });
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(300)
                    .HasLink("Dashboard", "ExternalRanklist", "List")
                    .HasTitle(string.Empty, "External OJ Ranklist")
                    .RequireRoles("Administrator,TeamLeader");
            });
        }
    }
}

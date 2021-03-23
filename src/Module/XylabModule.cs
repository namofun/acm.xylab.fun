﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.Services;
using SatelliteSite.XylabModule.Services;

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
                options.PointBeforeUrlRewriting.Add(app => app.UseMiddleware<AliyunCdnRealIpMiddleware>());
            });

            services.ConfigureIdentityAdvanced(options =>
            {
                options.ShortenedClaimName = true;
            });

            services.EnsureSingleton<IEmailSender>();
            services.ReplaceSingleton<IEmailSender, HybridEmailSender>();
            services.AddOptions<HybridEmailOptions>()
                .Bind(configuration.GetSection("SendGrid"));
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.MainNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasTitle("fas fa-home", "Home")
                    .HasLink("Xylab", "Home", "Index")
                    .ActiveWhenViewData("HomePage");

                menu.HasEntry(200)
                    .HasTitle("fas fa-book-open", "Problems")
                    .HasLink("Xylab", "Home", "Problems")
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

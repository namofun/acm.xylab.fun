using Microsoft.AspNetCore.Authorization;
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

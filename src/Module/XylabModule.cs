using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SatelliteSite.XylabModule
{
    public class XylabModule : AbstractModule, IAuthorizationPolicyRegistry
    {
        public override string Area => "Xylab";

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.ConfigureApplicationBuilder(options =>
            {
                options.SiteName = "Online Judge";
                options.PointBeforeUrlRewriting.Add(app => app.UseMiddleware<AliyunCdnRealIpMiddleware>());
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

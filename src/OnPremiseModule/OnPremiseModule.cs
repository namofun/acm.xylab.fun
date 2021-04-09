using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SatelliteSite.OnPremiseModule
{
    public class OnPremiseModule : AbstractModule
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
            });

            services.ConfigureIdentityAdvanced(options =>
            {
                options.ShortenedClaimName = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.OnAppendCookie = ctx =>
                {
                    if (ctx.CookieName == ".AspNet.Tenant.CurrentTenant")
                    {
                        ctx.CookieOptions.SameSite = SameSiteMode.Lax;
                    }
                };
            });
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.WithErrorHandler("Xylab", "Home")
                .MapStatusCode("/{**slug}");
        }
    }
}

using Microsoft.AspNetCore.Builder;
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
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.MainNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasTitle("fas fa-home", "Home")
                    .HasLink("/")
                    .ActiveWhenViewData("HomePage");

                menu.HasEntry(200)
                    .HasTitle("fas fa-graduation-cap", "Teach")
                    .HasLink("Xylab", "Home", "Teach")
                    .ActiveWhenViewData("Teacher");
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

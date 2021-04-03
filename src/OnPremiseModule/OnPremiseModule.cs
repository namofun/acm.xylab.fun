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
                    .HasLink("Xylab", "Home", "Index")
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
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.WithErrorHandler("Xylab", "Home")
                .MapStatusCode("/{**slug}");
        }
    }
}

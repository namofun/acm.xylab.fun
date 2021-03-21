using Ccs.Registration;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace SatelliteSite
{
    public class HostModule : AbstractModule
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddMarkdown();

            services.AddDbModelSupplier<DefaultContext, Polygon.Storages.PolygonIdentityEntityConfiguration<XylabUser, DefaultContext>>();

            services.ConfigurePolygonStorage(options =>
            {
                options.JudgingDirectory = Path.Combine(environment.ContentRootPath, "Runs");
                options.ProblemDirectory = Path.Combine(environment.ContentRootPath, "Problems");
            });

            services.AddContestRegistrationTenant();
        }
    }
}

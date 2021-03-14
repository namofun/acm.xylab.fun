using Ccs.Registration;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polygon.FakeJudgehost;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class HostModule : AbstractModule
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapFallback("/", context =>
            {
                context.Response.Redirect("/dashboard/contests");
                return Task.CompletedTask;
            });
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

            services.AddFakeJudgehost()
                .AddFakeSeeds<DefaultContext>()
                .AddJudgehost<FakeJudgeActivity>("fake-judgehost-0")
                .AddHttpClientFactory(_ => new System.Net.Http.HttpClient { BaseAddress = new System.Uri("http://localhost:9121/api/") });
        }
    }
}

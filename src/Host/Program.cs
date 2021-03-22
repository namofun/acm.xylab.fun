using Ccs.Registration;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddModule<IdentityModule.IdentityModule<XylabUser, Role, DefaultContext>>()
                .AddModule<XylabModule.XylabModule>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<XylabUser, Role, DefaultContext>>()
                .AddModule<OjUpdateModule.OjUpdateModule<DefaultContext>>()
                .AddModule<NewsModule.NewsModule<DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<ContestModule.ContestModule<Ccs.RelationalRole<XylabUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<Plag.Backend.RestfulBackendRole>>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>()
                .ConfigureServices((context, services) =>
                {
                    services.AddMarkdown();

                    services.AddDbModelSupplier<DefaultContext, Polygon.Storages.PolygonIdentityEntityConfiguration<XylabUser, DefaultContext>>();

                    services.ConfigurePolygonStorage(options =>
                    {
                        options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                        options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                    });

                    services.AddContestRegistrationTenant();
                });
    }
}

using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.MigratePolygonV1();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddApplicationInsights()
                .AddAzureBlobWebRoot((c, o) => { })
                .AddModule<IdentityModule.IdentityModule<XylabUser, Role, DefaultContext>>()
                .EnableIdentityModuleBasicAuthentication()
                .AddModule<XylabModule.XylabModule>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<XylabUser, Role, DefaultContext>>()
                .AddModule<NewsModule.NewsModule<DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Xylab.Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<ContestModule.ContestModule<Xylab.Contesting.RelationalRole<XylabUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<Xylab.PlagiarismDetect.Backend.CosmosBackendRole>>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbModelSupplier<DefaultContext, Xylab.Polygon.Storages.PolygonIdentityEntityConfiguration<XylabUser, DefaultContext>>();
                    services.AddSingleton<ConnectionCache>();
                    services.AddSingleton<Xylab.PlagiarismDetect.Backend.Services.ISignalProvider, StorageQueueSignalProvider>();
                    services.ConfigureOptions<ConfigureConnections>();
                    services.Configure<DefaultAzureCredentialOptions>(options => options.VisualStudioTenantId = "65f7f058-fc47-4803-b7f6-1dd03a071b50");
                });
    }
}

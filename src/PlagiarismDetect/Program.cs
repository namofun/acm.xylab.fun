using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SatelliteSite
{
    public static class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args)
                .Build()
                .AutoMigrate<PdsContext>()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<PdsContext>()
                .AddModule<PlagModule.PlagModule<Plag.Backend.StorageBackendRole<PdsContext>>>()
                .AddDatabase<PdsContext>((c, b) => b.UseSqlServer(c.GetConnectionString("PlagDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaultsCore();
    }
}

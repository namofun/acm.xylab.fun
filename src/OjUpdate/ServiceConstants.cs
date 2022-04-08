using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace Xylab.BricksService.OjUpdate
{
    public static class ServiceConstants
    {
        public static IUpdateDriver GetDriver(RecordType category)
        {
            return category switch
            {
                RecordType.Hdoj => new HdojDriver(),
                RecordType.Codeforces => new CodeforcesDriver(),
                RecordType.Vjudge => new VjudgeDriver(),
                _ => throw new NotSupportedException(),
            };
        }

        public static IEnumerable<KeyValuePair<RecordType, IUpdateDriver>> GetDrivers()
        {
            yield return new(RecordType.Hdoj, new HdojDriver());
            yield return new(RecordType.Codeforces, new CodeforcesDriver());
            yield return new(RecordType.Vjudge, new VjudgeDriver());
        }

        public static IServiceCollection AddOjUpdateRemoteService(this IServiceCollection services)
        {
            services.AddSingleton<IUpdateProvider, RemoteServiceUpdateProvider>();
            return services;
        }

        public static IServiceCollection AddOjUpdateBackgroundService(this IServiceCollection services, TimeSpan sleepLength)
        {
            OjUpdateService.SleepLength = (int)sleepLength.TotalSeconds;
            services.AddSingleton<IUpdateProvider, BackgroundServiceUpdateProvider>();
            foreach ((_, IUpdateDriver driver) in ServiceConstants.GetDrivers())
            {
                services.AddSingleton<IHostedService>(sp => new OjUpdateService(sp, driver));
            }

            return services;
        }

        public static IServiceCollection AddOjUpdateCosmosDbStore(this IServiceCollection services)
        {
            services.AddSingleton<RecordV2Storage>();
            services.AddSingletonUpcast<IRecordStorage, RecordV2Storage>();
            return services;
        }

        public static IServiceCollection AddOjUpdateEntityFrameworkCoreStore<TContext>(this IServiceCollection services) where TContext : DbContext
        {
            services.AddScoped<IRecordStorage, RecordV1Storage<TContext>>();
            services.AddDbModelSupplier<TContext, RecordV1EntityConfiguration<TContext>>();
            return services;
        }
    }
}

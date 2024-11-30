using Microsoft.Extensions.DependencyInjection;
using Realms;
using Serilog;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb;

public static class ServicesRealmDBRegistrationExtension
{
    public static IServiceCollection AddRealmDBLog(this IServiceCollection services)
    {
        services.AddSingleton<RealmConfiguration>(RealmDBExtension.DefaultConfig());
        services.AddSingleton<ILogStore, RealmLoggingDbCtx>();

        return services;
    }
}

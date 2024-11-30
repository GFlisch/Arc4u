using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Diagnostics.Serilog.Sinks.Memory;

public static class ServicesMemoryRegistrationExtension
{
    public static IServiceCollection AddMemoryLogDB(this IServiceCollection services)
    {
        services.AddSingleton<MemoryLogMessages>(MemoryLogDbSink.LogMessages);
        services.AddSingleton<ILogStore, MemoryLogStore>();

        return services;
    }
}

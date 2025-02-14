using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration.Store;

using Internals;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// This needs to be called AFTER the settings store (e.g. the Database Context) is fully defined
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IServiceProvider UseSectionStoreConfiguration(this IServiceProvider serviceProvider)
    {
        // the factory is a singleton, which is what we want/
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var sectionStoreService = serviceProvider.GetRequiredService<ISectionStoreService>();
        sectionStoreService.Startup(serviceScopeFactory);
        return serviceProvider;
    }
}

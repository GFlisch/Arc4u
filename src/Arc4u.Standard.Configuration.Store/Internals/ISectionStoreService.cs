using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration.Store.Internals;

/// <summary>
/// Services implementing this interface can be singleton
/// </summary>
interface ISectionStoreService
{
    void Startup(IServiceScopeFactory serviceScopeFactory);
    void ReloadIfNeeded();
}

using System;

namespace Arc4u.OAuth2;

/// <summary>
/// </summary>
public interface IScopedServiceProviderAccessor
{
    IServiceProvider ServiceProvider { get; set; }
}

using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency.ComponentModel;

public class DependencyFactory : IServiceProviderFactory<IServiceProvider>
{
    public IServiceProvider CreateBuilder(IServiceCollection services)
    {
        var container = new ComponentModelContainer(services);

        container.CreateContainer();

        return container;
    }

    public IServiceProvider CreateServiceProvider(IServiceProvider containerBuilder)
    {
        return containerBuilder;
    }
}

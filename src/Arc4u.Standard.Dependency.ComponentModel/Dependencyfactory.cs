using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arc4u.Dependency.ComponentModel
{
    [Obsolete("Consider using NamedServiceProviderFactory, especially if you want provider construction options in addition to named services")]
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
}

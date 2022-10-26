using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arc4u.Dependency.ComponentModel
{
    public class NamedServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly ServiceProviderOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyFactory"/> class with default options.
        /// </summary>
        public NamedServiceProviderFactory() 
            : this(NamedServiceCollectionContainerBuilderExtensions.DefaultServiceProviderOptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyFactory"/> class with the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options to use for this instance.</param>
        public NamedServiceProviderFactory(ServiceProviderOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }


        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            services.AddNamedServicesSupport();
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildNamedServiceProvider(_options);
        }
    }
}

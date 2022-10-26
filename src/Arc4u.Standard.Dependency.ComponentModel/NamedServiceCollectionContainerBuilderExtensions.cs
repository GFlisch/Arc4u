using Microsoft.Extensions.DependencyInjection;
using System;

namespace Arc4u.Dependency.ComponentModel
{
    public static class NamedServiceCollectionContainerBuilderExtensions
    {
        public static readonly ServiceProviderOptions DefaultServiceProviderOptions = new ServiceProviderOptions();

        /// <summary>
        /// Creates a <see cref="NamedServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <returns>The <see cref="NamedServiceProvider"/>.</returns>

        public static INamedServiceProvider BuildNamedServiceProvider(this IServiceCollection services)
        {
            return BuildNamedServiceProvider(services, DefaultServiceProviderOptions);
        }

        /// <summary>
        /// Creates a <see cref="NamedServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="validateScopes">
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
        /// </param>
        /// <returns>The <see cref="NamedServiceProvider"/>.</returns>
        public static INamedServiceProvider BuildNamedServiceProvider(this IServiceCollection services, bool validateScopes)
        {
            return services.BuildNamedServiceProvider(new ServiceProviderOptions { ValidateScopes = validateScopes });
        }

        /// <summary>
        /// Creates a <see cref="NamedServiceProvider"/> containing services from the provided <see cref="IServiceCollection"/>
        /// optionally enabling scope validation.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing service descriptors.</param>
        /// <param name="options">
        /// Configures various service provider behaviors.
        /// </param>
        /// <returns>The <see cref="NamedServiceProvider"/>.</returns>
        public static INamedServiceProvider BuildNamedServiceProvider(this IServiceCollection services, ServiceProviderOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return new NamedServiceProvider(services, options);
        }
    }
}

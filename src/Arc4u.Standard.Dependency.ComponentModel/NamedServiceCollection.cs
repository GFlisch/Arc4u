using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency.ComponentModel
{
    public static class NamedServiceCollectionExtensionMethods
    {
        /// <summary>
        /// Call this before adding named services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNamedServicesSupport(this IServiceCollection services)
        {
            var resolver = services.NameResolver();
            if (resolver == null)
                services.AddSingleton<INameResolver>(new NameResolver());
            return services;
        }             
    }
}

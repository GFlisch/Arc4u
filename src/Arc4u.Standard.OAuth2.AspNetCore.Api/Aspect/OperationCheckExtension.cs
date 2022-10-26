using Arc4u.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.OAuth2.Aspect
{
    public static class OperationCheckExtension
    {
        public static void AddAuthorizationCheckAttribute(this IServiceCollection services)
        {
            services.TryAddScoped<OperationCheckAttribute>();
        }
    }
}

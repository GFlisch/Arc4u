using System;
using Arc4u.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.OAuth2.Aspect
{
    public static class OperationCheckExtension
    {
        [Obsolete("Use ManageExceptionsFilter and SetCultureActionFilter instead.")]

        public static void AddAuthorizationCheckAttribute(this IServiceCollection services)
        {
            services.TryAddScoped<OperationCheckAttribute>();
        }

        [Obsolete("Use ManageExceptionsFilter and SetCultureActionFilter instead.")]
        public static void AddAuthorizationCheckAttribute(this IContainer container)
        {
            container.RegisterScoped<OperationCheckAttribute, OperationCheckAttribute>();
        }
    }
}

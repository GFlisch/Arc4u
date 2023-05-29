using Arc4u.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Blazor
{
    public static class OperationsPolicyExtension
    {
        public static void RegisterOperationsPolicy(this IServiceCollection services, IEnumerable<KeyValuePair<int, string>> operations, Action<AuthorizationOptions> authorizationOptions = null)
        {
            services.AddSingleton<IAuthorizationHandler, ScopedOperationsHandler>();

            if (null != authorizationOptions)
                services.Configure(authorizationOptions);

            services.AddAuthorizationCore(options =>
            {
                foreach (var operation in operations)
                {
                    options.AddPolicy(operation.Value, policy =>
                    policy.Requirements.Add(new ScopedOperationsRequirement(operation.Key)));
                }
            });
        }
    }
}


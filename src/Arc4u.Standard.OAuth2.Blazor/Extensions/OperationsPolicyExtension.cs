using Arc4u.Standard.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace Arc4u.Blazor
{
    public static class OperationsPolicyExtension
    {
        public static void RegisterOperationsPolicy(this IServiceCollection services, IEnumerable<KeyValuePair<int, string>> operations, Action<AuthorizationOptions> authorizationOptions = null)
        {
            services.TryAddSingleton<IAuthorizationHandler, OperationsHandler>();

            if (null != authorizationOptions)
                services.Configure(authorizationOptions);

            services.AddAuthorizationCore(options =>
            {
                foreach (var operation in operations)
                {
                    options.AddPolicy(operation.Value, policy =>
                    policy.Requirements.Add(new OperationRequirement(operation.Key)));
                }
            });
        }
    }
}


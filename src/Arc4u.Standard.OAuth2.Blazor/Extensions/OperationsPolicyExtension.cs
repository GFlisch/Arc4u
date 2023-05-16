using Arc4u.Standard.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Blazor
{
    /// <summary>
    /// Provides extension methods for registering operation policies.
    /// </summary>
    public static class OperationsPolicyExtension
    {
        /// <summary>
        /// Registers a set of operation policies in the provided services collection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the operation policies to.</param>
        /// <param name="operations">A set of key-value pairs where each key is an operation ID and each value is a corresponding policy name.</param>
        /// <param name="authorizationOptions">Optional action to further configure the AuthorizationOptions.</param>
        public static void RegisterOperationsPolicy(this IServiceCollection services, IEnumerable<KeyValuePair<int, string>> operations, Action<AuthorizationOptions> authorizationOptions = null)
        {
            services.AddSingleton<IAuthorizationHandler, OperationsHandler>();

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


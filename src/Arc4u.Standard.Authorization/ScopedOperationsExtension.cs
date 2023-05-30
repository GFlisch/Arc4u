using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Authorization;
public static class ScopedOperationsExtension
{
    public static void AddScopedOperationsPolicy(this IServiceCollection services, IEnumerable<string> scopes, IEnumerable<Operation> operations, Action<AuthorizationOptions> authorizationOptions = null)
    {
        services.AddScoped<IAuthorizationHandler, ScopedOperationsHandler>();

        if (authorizationOptions is not null)
        {
            services.Configure(authorizationOptions);
        }

        services.AddAuthorizationCore(options =>
        {
            // Add the default policy.
            foreach (var operation in operations)
            {
                options.AddPolicy(operation.Name, policy =>
                policy.Requirements.Add(new ScopedOperationsRequirement(operation.ID)));
            }

            // Add the scoped policy.
            foreach (var scope in scopes)
            {
                foreach (var operation in operations)
                {
                    options.AddPolicy($"{scope}:{operation.Name}", policy =>
                    policy.Requirements.Add(new ScopedOperationsRequirement(scope, operation.ID)));
                }
            }
        });
    }
}

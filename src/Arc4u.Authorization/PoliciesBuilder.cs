using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Authorization;

/// <summary>
/// A fluent builder for registering additional authorization policies and configuring
/// <see cref="AuthorizationOptions"/> after scoped-operation policies have been set up
/// by <see cref="ScopedOperationsExtension.AddScopedOperationsPolicy"/>.
/// </summary>
/// <remarks>
/// Instances are created internally by
/// <see cref="ScopedOperationsExtension.AddScopedOperationsPolicy"/> and returned to
/// the caller so that extra policies or global authorization settings can be chained
/// in a single fluent expression.
/// </remarks>
public sealed class PoliciesBuilder
{
    private readonly IServiceCollection _services;

    internal PoliciesBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers a named authorization policy built with the supplied delegate.
    /// </summary>
    /// <param name="name">
    /// The unique name of the policy. Must not be <see langword="null"/> or empty.
    /// </param>
    /// <param name="configure">
    /// A delegate that configures the <see cref="AuthorizationPolicyBuilder"/> for the new policy.
    /// </param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public PoliciesBuilder AddPolicy(string name, Action<AuthorizationPolicyBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(configure);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, configure));
        return this;
    }

    public PoliciesBuilder AddAndOperations(string name, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new ScopedOperationsRequirement(operations))));

        return this;
    }

    public PoliciesBuilder AddAndOperations(string name, string scope, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(scope);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new ScopedOperationsRequirement(scope, operations))));

        return this;
    }

    /// <summary>
    /// Applies additional configuration to the global <see cref="AuthorizationOptions"/>,
    /// such as setting <see cref="AuthorizationOptions.DefaultPolicy"/> or
    /// <see cref="AuthorizationOptions.InvokeHandlersAfterFailure"/>.
    /// </summary>
    /// <param name="configure">
    /// A delegate that configures the <see cref="AuthorizationOptions"/>.
    /// </param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public PoliciesBuilder ConfigureAuthorization(Action<AuthorizationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        _services.Configure(configure);
        return this;
    }
}

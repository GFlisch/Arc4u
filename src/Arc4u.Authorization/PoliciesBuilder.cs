using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Authorization;

/// <summary>
/// A fluent builder, returned by <see cref="ScopedOperationsExtension.AddScopedOperationsPolicy"/>,
/// for registering additional authorization policies and tweaking
/// <see cref="AuthorizationOptions"/> in a single chained expression.
/// </summary>
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
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="configure">A delegate that configures the <see cref="AuthorizationPolicyBuilder"/> for the new policy.</param>
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

    /// <summary>
    /// Registers a policy that is satisfied only when the current principal is granted
    /// <b>all</b> of the supplied <paramref name="operations"/> in the default (empty) scope.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="operations">One or more operation identifiers that must <b>all</b> be granted.</param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAllOperations(string name, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new AllScopedOperationsRequirement(operations))));

        return this;
    }

    /// <summary>
    /// Registers a policy that is satisfied only when the current principal is granted
    /// <b>all</b> of the supplied <paramref name="operations"/> within the given <paramref name="scope"/>.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="scope">The scope every operation must be granted in. Must not be <see langword="null"/> or empty.</param>
    /// <param name="operations">One or more operation identifiers that must <b>all</b> be granted within <paramref name="scope"/>.</param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> or <paramref name="scope"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAllOperations(string name, string scope, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(scope);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new AllScopedOperationsRequirement(scope, operations))));

        return this;
    }

    /// <summary>
    /// Registers a policy that is satisfied only when the current principal is granted
    /// <b>all</b> of the supplied (scope, operation) pairs, each within its own scope.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="scopedOperations">
    /// One or more (scope, operation) tuples. Use <see cref="string.Empty"/> for the default scope.
    /// </param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAllOperations(string name, params (string scope, int operation)[] scopedOperations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new AllScopedOperationsRequirement(scopedOperations))));

        return this;
    }

    /// <summary>
    /// Registers a policy that is satisfied as soon as the current principal is granted
    /// <b>any</b> one of the supplied <paramref name="operations"/> in the default (empty) scope.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="operations">One or more operation identifiers; being granted any of them satisfies the policy.</param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAnyOperation(string name, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new AnyScopedOperationRequirement(operations))));

        return this;
    }

    /// <summary>
    /// Registers a policy that is satisfied as soon as the current principal is granted
    /// <b>any</b> one of the supplied <paramref name="operations"/> within the given <paramref name="scope"/>.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="scope">The scope in which to check the operations. Must not be <see langword="null"/> or empty.</param>
    /// <param name="operations">One or more operation identifiers; being granted any of them within <paramref name="scope"/> satisfies the policy.</param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> or <paramref name="scope"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAnyOperation(string name, string scope, params int[] operations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(scope);

        _services.Configure<AuthorizationOptions>(opts => opts.AddPolicy(name, p => p.AddRequirements(new AnyScopedOperationRequirement(scope, operations))));

        return this;
    }

    /// <summary>
    /// Registers a policy that is satisfied as soon as the current principal is granted
    /// <b>any</b> one of the supplied (scope, operation) pairs.
    /// </summary>
    /// <param name="name">The unique name of the policy. Must not be <see langword="null"/> or empty.</param>
    /// <param name="scopedOperations">
    /// One or more (scope, operation) tuples; being granted any pair satisfies the policy.
    /// Use <see cref="string.Empty"/> for the default scope.
    /// </param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null"/> or empty.</exception>
    public PoliciesBuilder AddAnyOperation(string name, params (string scope, int operation)[] scopedOperations)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _services.Configure<AuthorizationOptions>(opts =>
            opts.AddPolicy(name, p => p.AddRequirements(new AnyScopedOperationRequirement(scopedOperations))));

        return this;
    }

    /// <summary>
    /// Applies additional configuration to the global <see cref="AuthorizationOptions"/>,
    /// such as setting <see cref="AuthorizationOptions.DefaultPolicy"/> or
    /// <see cref="AuthorizationOptions.InvokeHandlersAfterFailure"/>.
    /// </summary>
    /// <param name="configure">A delegate that configures the <see cref="AuthorizationOptions"/>.</param>
    /// <returns>The same <see cref="PoliciesBuilder"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configure"/> is <see langword="null"/>.</exception>
    public PoliciesBuilder ConfigureAuthorization(Action<AuthorizationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        _services.Configure(configure);
        return this;
    }
}

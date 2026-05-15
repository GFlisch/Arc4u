using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

/// <summary>
/// Represents an authorization requirement that is satisfied as soon as the current principal
/// is granted <b>any</b> one of the specified scoped operations (OR logic).
/// </summary>
/// <remarks>
/// Each operation is an integer identifier (typically an <see cref="Security.Principal.Operation.ID"/>)
/// paired with a <em>scope</em> — a free-form string used to partition permissions
/// (e.g. a tenant or feature area). An empty scope (<see cref="string.Empty"/>) is the default
/// and matches operations granted without an explicit scope.
/// </remarks>
public class AnyScopedOperationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance using the default (empty) scope for every operation.
    /// </summary>
    /// <param name="operations">One or more operation identifiers.</param>
    public AnyScopedOperationRequirement(params int[] operations)
    {
        Operations = operations.Select(o => (string.Empty, o)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance where every operation is paired with the same <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The scope applied to every operation. Must not be <see langword="null"/>.</param>
    /// <param name="operations">One or more operation identifiers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scope"/> is <see langword="null"/>.</exception>
    public AnyScopedOperationRequirement(string scope, params int[] operations)
    {
        ArgumentNullException.ThrowIfNull(scope);

        Operations = operations.Select(o => (scope, o)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance with explicit (scope, operation) pairs,
    /// allowing each operation to belong to a different scope.
    /// </summary>
    /// <param name="operations">
    /// One or more (scope, operation) tuples. Each scope must be non-<see langword="null"/>;
    /// use <see cref="string.Empty"/> for the default scope.
    /// </param>
    public AnyScopedOperationRequirement(params (string, int)[] operations)
    {
        Operations = operations;
    }

    /// <summary>
    /// Gets the (scope, operation) pairs from which at least <b>one</b> must be granted to the
    /// current principal for the requirement to be satisfied.
    /// </summary>
    public (string scope, int operation)[] Operations { get; private set; }
}

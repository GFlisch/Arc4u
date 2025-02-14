using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

public class ScopedOperationsRequirement : IAuthorizationRequirement
{
    public ScopedOperationsRequirement(params int[] operations)
    {
        _operations = operations;
    }
    public ScopedOperationsRequirement([DisallowNull] string scope, params int[] operations)
    {
        ArgumentNullException.ThrowIfNull(scope);

        _scope = scope;
        _operations = operations;
    }

    private readonly string _scope = string.Empty;
    private readonly int[] _operations;

    public int[] Operations => _operations;

    public string Scope => _scope;
}

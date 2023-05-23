using System.Diagnostics.CodeAnalysis;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Security;

/// <summary>
/// This requirement is used to check if the user has the right to access a resource based on the operations and or the scope.
/// </summary>
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

using System.Globalization;

namespace Arc4u.Security.Principal;

public class AppAuthorization : IAuthorization
{
    private readonly Dictionary<string, Dictionary<int, string>> _operations;
    private readonly Dictionary<string, Dictionary<string, int>> _operationsName;
    private readonly Dictionary<string, Dictionary<string, short>> _roles;
    private readonly List<string> _scopes;

    public AppAuthorization(Authorization authorizationData)
    {
        // Fill the Dictionnary structure for fast retrieving the information.
        // Add Operations
        _operations = new Dictionary<string, Dictionary<int, string>>();
        _operationsName = new Dictionary<string, Dictionary<string, int>>();
        foreach (var scopedOperations in authorizationData.Operations)
        {
            var operations = new Dictionary<int, string>();
            var operationsName = new Dictionary<string, int>();

            foreach (var operationId in scopedOperations.Operations)
            {
                var operation = authorizationData.AllOperations.SingleOrDefault(o => o.ID == operationId);
                if (default(Operation) != operation)
                {
                    operations.Add(operation.ID, operation.Name);
                    operationsName.Add(operation.Name, operation.ID);
                }
            };
            _operations.Add(scopedOperations.Scope, operations);
            _operationsName.Add(scopedOperations.Scope, operationsName);
        }

        // Add Roles.
        _roles = new Dictionary<string, Dictionary<string, short>>();
        foreach (var scopedRoles in authorizationData.Roles)
        {
            var roles = new Dictionary<string, short>();
            foreach (var role in scopedRoles.Roles)
            {
                roles.Add(role, 0);
            }

            _roles.Add(scopedRoles.Scope, roles);
        }

        _scopes = authorizationData.Scopes;
    }

    #region IAuthorization Members

    public string AuthorizationType
    {
        get { return "Arc4uAuthorization"; }
    }

    public string[] Scopes()
    {
        return _scopes.ToArray();
    }

    public string[] Roles()
    {
        return Roles(string.Empty);
    }

    public string[] Roles(string scope)
    {
        try
        {
            var roles = new string[_roles[scope].Count];
            _roles[scope].Keys.CopyTo(roles, 0);

            return roles;
        }
        catch
        {
            return [];
        }
    }

    public bool IsAuthorized(params int[] operations)
    {
        return IsAuthorized(string.Empty, operations);
    }

    public bool IsAuthorized(params string[] operations)
    {
        return IsAuthorized(string.Empty, operations);
    }

    public bool IsAuthorized(string scope, params int[] operations)
    {
        if (_operations.ContainsKey(scope))
        {
            foreach (var i in operations)
            {
                if (!_operations[scope].ContainsKey(i))
                {
                    return false;
                }
            }
        }
        else // No Scope no Operations.
        {
            return false;
        }

        return true;
    }

    public bool IsAuthorized(string scope, params string[] operations)
    {
        if (_operationsName.ContainsKey(scope))
        {
            foreach (var o in operations)
            {
                if (!_operationsName[scope].ContainsKey(o))
                {
                    return false;
                }
            }

        }
        else // No Scope no Operations.
        {
            return false;
        }

        return true;
    }
    public bool IsAuthorized<TAccess>(params TAccess[] operations) where TAccess : struct, Enum
    {
        return IsAuthorized(string.Empty, operations);
    }

    public bool IsAuthorized<TAccess>(string scope, params TAccess[] operations)
           where TAccess : struct, Enum
    {
        var ids = new int[operations.Length];

        for (var index = 0; index < operations.Length; ++index)
        {
            ids[index] = Convert.ToInt32(operations[index], CultureInfo.InvariantCulture);
        }

        return IsAuthorized(scope, ids);
    }

    public bool IsInRole(string role)
    {
        return IsInRole(string.Empty, role);
    }

    public bool IsInRole(string scope, string role)
    {
        if (_roles.TryGetValue(scope, out var roles))
        {
            return roles.ContainsKey(role);
        }

        return false;
    }

    public string[] Operations()
    {
        return Operations(string.Empty);
    }

    public string[] Operations(string scope)
    {
        try
        {
            var operations = new string[_operations[scope].Count];
            _operations[scope].Values.CopyTo(operations, 0);

            return operations;
        }
        catch
        {
            return [];
        }
    }

    #endregion
}

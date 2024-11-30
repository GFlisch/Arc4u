using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace Arc4u.Security.Principal;

/// <summary>
/// AppPrincipal is the principal which knows all the information on a person (authorization, authentication and profile information).
/// We use a principal because it is copied from thrad to thead (new in .Net 2.0).
/// </summary>
public class AppPrincipal : ClaimsPrincipal, IAuthorization
{
    private readonly AppAuthorization _authorization;
    private string _sid;
    /// <summary>
    /// Initializes a new instance of the <see cref="AppPrincipal"/> class.
    /// </summary>
    /// <param name="authorizationData">The authorization data.</param>
    /// <param name="identity">The identity.</param>
    /// <param name="sid">The sid.</param>
    public AppPrincipal(Authorization authorizationData, IIdentity identity, string sid) : base(identity)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(sid);
        ArgumentNullException.ThrowIfNull(authorizationData);
#else
        if (null == identity)
        {
            throw new ArgumentNullException(nameof(identity));
        }
        if (null == sid)
        {
            throw new ArgumentNullException(nameof(sid));
        }
        if (null == authorizationData)
        {
            throw new ArgumentNullException(nameof(authorizationData));
        }
#endif
        _sid = sid;
        Authorization = authorizationData;
        _authorization = new AppAuthorization(authorizationData);
        profile = new UserProfile();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppPrincipal"/> class.
    /// </summary>
    private AppPrincipal()
    {
        _authorization = new AppAuthorization(new Authorization());
        Authorization = new Authorization();
        _sid = string.Empty;
        profile = new UserProfile();
    }

    #region IPrincipal Members

    /// <exclude/>
    public override bool IsInRole(string role)
    {
        return _authorization.IsInRole(role);
    }

    #endregion

    #region IAuthorization Members

    /// <exclude/>
    public bool IsInRole(string scope, string role)
    {
        return _authorization.IsInRole(scope, role);
    }

    /// <exclude/>
    public string AuthorizationType
    {
        get { return "AppPrincipal"; }
    }

    /// <exclude/>
    public string[] Scopes()
    {
        return _authorization.Scopes();
    }

    /// <exclude/>
    public string[] Roles()
    {
        return _authorization.Roles();
    }

    /// <exclude/>
    public string[] Roles(string scope)
    {
        return _authorization.Roles(scope);
    }

    /// <exclude/>
    public string[] Operations()
    {
        return _authorization.Operations();
    }

    /// <exclude/>
    public string[] Operations(string scope)
    {
        return _authorization.Operations(scope);
    }

    /// <exclude/>
    public bool IsAuthorized(params int[] operations)
    {
        return _authorization.IsAuthorized(operations);
    }

    public bool IsAuthorized<TAccess>(params TAccess[] operations)
       where TAccess : struct, Enum
    {
        var ids = new int[operations.Length];
        for (var index = 0; index < operations.Length; ++index)
        {
            ids[index] = Convert.ToInt32(operations[index], CultureInfo.InvariantCulture);
        }

        return _authorization.IsAuthorized(ids);
    }

    /// <exclude/>
    public bool IsAuthorized(string scope, params int[] operations)
    {
        return _authorization.IsAuthorized(scope, operations);
    }

    /// <exclude/>
    public bool IsAuthorized(params string[] operations)
    {
        return _authorization.IsAuthorized(operations);
    }

    /// <exclude/>
    public bool IsAuthorized(string scope, params string[] operations)
    {
        return _authorization.IsAuthorized(scope, operations);
    }

    #endregion

    /// <exclude/>
    public Authorization Authorization { get; }

    // If user profile is 
    private UserProfile profile;
    /// <summary>
    /// Gets or sets the profile.
    /// </summary>
    /// <value>The profile.</value>
    public UserProfile Profile
    {
        get
        {
            return profile;
        }
        set
        {
            profile = value;
            _sid = profile.Sid;
        }
    }

    public string Sid { get => _sid; }
}

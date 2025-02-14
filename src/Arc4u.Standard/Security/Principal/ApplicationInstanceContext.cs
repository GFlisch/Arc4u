namespace Arc4u.Security.Principal;

/// <summary>
/// This class must be registered manually in your DI container.
/// 1) as Singleton when used:
///     a) in UI.
///     b) in one instance application context
///     c) unit test.
/// 2) as Scoped in AspNetCore backend or backend unit test..
/// </summary>
public class ApplicationInstanceContext : IApplicationContext
{
    public void SetPrincipal(AppPrincipal principal)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(principal);
#else
        if (null == principal)
        {
            throw new ArgumentNullException(nameof(principal));
        }
#endif
        Principal = principal;
    }

    /// <summary>
    /// Gets or sets the activity ID.
    /// </summary>
    /// <value>The activity ID.</value>
    public string ActivityID { get; set; } = string.Empty;

    public AppPrincipal? Principal { get; private set; }
}

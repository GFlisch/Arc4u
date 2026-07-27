using System.Security.Claims;
using Arc4u.Security.Principal;

namespace Arc4u.Blazor;

/// <summary>
/// Builds an <see cref="AppPrincipal"/> from a <see cref="ClaimsIdentity"/> and pushes it into the
/// (scope-bound) <see cref="IApplicationContext"/>. This is the shared body used both by
/// <see cref="AppPrincipalServerAuthenticationStateProvider"/> (prerender / SSR request scope) and by
/// <see cref="ApplicationContextCircuitHandler"/> (interactive circuit scope), mirroring the WASM
/// <c>Arc4u.Blazor.AppPrincipalFromAuthenticationState</c>.
/// </summary>
internal static class ApplicationContextInitializer
{
    public static AppPrincipal? SetPrincipal(
        ClaimsIdentity? identity,
        IClaimAuthorizationFiller authorizationFiller,
        IClaimProfileFiller profileFiller,
        IApplicationContext applicationContext)
    {
        // Nothing to build for an anonymous user: leave the context untouched.
        if (identity is null || !identity.IsAuthenticated)
        {
            return null;
        }

        var authorization = authorizationFiller.GetAuthorization(identity);
        var profile = profileFiller.GetProfile(identity);
        var principal = new AppPrincipal(authorization, identity, profile.Sid) { Profile = profile };

        applicationContext.SetPrincipal(principal);

        return principal;
    }
}


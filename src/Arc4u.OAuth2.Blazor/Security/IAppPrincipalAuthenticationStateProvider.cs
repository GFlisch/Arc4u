#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.Components.Authorization;

namespace Arc4u.Blazor;

public interface IAppPrincipalAuthenticationStateProvider
{
    Task<AuthenticationState> DeserializeAuthenticationStateAsync(AuthenticationStateData? authenticationStateData);
}
#endif

#if NET9_0_OR_GREATER
using Arc4u.Dependency.Attribute;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace Arc4u.Blazor.Options;

[Export(typeof(IConfigureOptions<AuthenticationStateDeserializationOptions>)), Shared]
public sealed class ConfigureAuthStateDeserializationOptions
    : IConfigureOptions<AuthenticationStateDeserializationOptions>
{
    private readonly IAppPrincipalAuthenticationStateProvider _mapper;

    public ConfigureAuthStateDeserializationOptions(IAppPrincipalAuthenticationStateProvider mapper)
    {
        _mapper = mapper;
    }

    public void Configure(AuthenticationStateDeserializationOptions options)
    {
        options.DeserializationCallback = async data =>
        {
            if (data is null)
            {
                return AppPrincipalFromAuthenticationState.DefaultAuthenticationState;
            }

            return await _mapper.DeserializeAuthenticationStateAsync(data).ConfigureAwait(false);
        };
    }
}
#endif

using Arc4u.Configuration;
using Microsoft.Extensions.Options;
using DefaultOptions = Microsoft.Extensions.Options.Options;

namespace Arc4u.OAuth2.Options;
public class PostConfigureOpenIdBearerInjectorSettings : IPostConfigureOptions<OpenIdBearerInjectorSettingsOptions>
{
    public PostConfigureOpenIdBearerInjectorSettings(IOptions<OpenIdBearerInjectorOptions> openIdOptions,
                                                     IOptionsMonitor<SimpleKeyValueSettings> simpleKeyOptions)
    {
        _opendIdOptions = openIdOptions.Value;
        _simpleKeyOptions = simpleKeyOptions;
    }

    readonly OpenIdBearerInjectorOptions _opendIdOptions;
    readonly IOptionsMonitor<SimpleKeyValueSettings> _simpleKeyOptions;

    public void PostConfigure(string? name, OpenIdBearerInjectorSettingsOptions options)
    {
        if (name == DefaultOptions.DefaultName)
        {
            options.OnBehalfOfOpenIdSettings = _simpleKeyOptions.Get(_opendIdOptions.OnBehalfOfOpenIdSettingsKey);
            options.OboProviderKey = _opendIdOptions.OboProviderKey;
            options.OpenIdSettings = _simpleKeyOptions.Get(_opendIdOptions.OpenIdSettingsKey);
        }
    }
}

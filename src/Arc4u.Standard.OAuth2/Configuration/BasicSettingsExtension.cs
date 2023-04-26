using System;
using System.Diagnostics.CodeAnalysis;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Standard.OAuth2.Extensions;
public static class BasicSettingsExtension
{
#if NET6_0_OR_GREATER
    public static SimpleKeyValueSettings AddBasicSettings(this IServiceCollection services, Action<BasicSettingsOptions> options, [DisallowNull] string sectionKey = "Basic")
#endif
#if NETSTANDARD2_0
    public static SimpleKeyValueSettings AddBasicSettings(this IServiceCollection services, Action<BasicSettingsOptions> options, string sectionKey = "Basic")
#endif
    {
        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }

        var validate = new BasicSettingsOptions();
        options(validate);
        var configErrors = string.Empty;

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            configErrors += "ProviderId field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.Audiences))
        {
            configErrors += "Audiences field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.Authority))
        {
            configErrors += "Authorithy field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.ClientId))
        {
            configErrors += "ClientId field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            configErrors += "AuthenticationType field is not defined." + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(configErrors))
        {
            throw new ConfigurationException(configErrors);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        void SettingsFiller(SimpleKeyValueSettings keyOptions)
        {
            keyOptions.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
            keyOptions.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);
            keyOptions.Add(TokenKeys.AuthorityKey, validate.Authority);
            keyOptions.Add(TokenKeys.ClientIdKey, validate.ClientId);
            keyOptions.Add(TokenKeys.Audiences, validate.Audiences);
        }

        services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);

        var settings = new SimpleKeyValueSettings();

        SettingsFiller(settings);

        return settings;
    }

#if NET6_0_OR_GREATER
    public static SimpleKeyValueSettings AddBasicSettings(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName = "Basic.Settings", [DisallowNull] string sectionKey = "Basic")
#endif
#if NETSTANDARD2_0
    public static SimpleKeyValueSettings AddBasicSettings(this IServiceCollection services, IConfiguration configuration, string sectionName = "Basic.Settings", string sectionKey = "Basic")
#endif
    {
        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }

        return AddBasicSettings(services, PrepareAction(configuration, sectionName), sectionKey);
    }

#if NET6_0_OR_GREATER
    internal static Action<BasicSettingsOptions> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
#endif
#if NETSTANDARD2_0
    internal static Action<BasicSettingsOptions> PrepareAction(IConfiguration configuration, string sectionName)
#endif
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        if (section is null)
        {
            throw new NullReferenceException($"No section exists with name {sectionName}");
        }

        var settings = section.Get<BasicSettingsOptions>() ?? throw new NullReferenceException($"No section exists with name {sectionName}");

        void OptionFiller(BasicSettingsOptions option)
        {
            option.Authority = settings.Authority;
            option.ClientId = settings.ClientId;
            option.Audiences = settings.Audiences;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
        }

        return OptionFiller;
    }
}

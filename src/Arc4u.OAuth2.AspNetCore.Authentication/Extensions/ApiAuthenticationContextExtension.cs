using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Extensions;

public static class ApiAuthenticationContextExtension
{
    public static void AddAuthenticationApiContext(this IServiceCollection services, Action<ApiExtraContextAuthenticationOption> option )
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(services);

        // Check if any configuration for this options type exists
        var hasConfiguration = services.Any(x =>
            x.ServiceType == typeof(IConfigureOptions<ApiExtraContextAuthenticationOption>) ||
            x.ServiceType == typeof(IPostConfigureOptions<ApiExtraContextAuthenticationOption>) ||
            x.ServiceType == typeof(IValidateOptions<ApiExtraContextAuthenticationOption>));

        if (!hasConfiguration)
        {
            services.Configure(option);
        }

    }

    public static void AddAuthenticationApiContext(this IServiceCollection services, IConfiguration configuration, ApiExtraContextAuthenticationSectionOption? section = null )
    {
        section ??= new ApiExtraContextAuthenticationSectionOption();

        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(services);

        void ApiExtraContextConfigurationFiller(ApiExtraContextAuthenticationOption option)
        {
            option.AuthorizationParameters = new CustomApiContextParameters(section.AuthorizationEndpointSectionPath, configuration);
            option.TokenParameters = new CustomApiContextParameters(section.TokenEndpointSectionPath, configuration);
        }

        AddAuthenticationApiContext(services, ApiExtraContextConfigurationFiller);
    }

    private class CustomApiContextParameters(string sectionName, IConfiguration configuration)
        : KeyValueSettings(sectionName, configuration);
}

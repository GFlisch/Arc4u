using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class TokenCacheExtension
{
    public static void AddTokenCache(this IServiceCollection services, Action<TokenCacheOptions> options)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var tokenCacheOptions = new TokenCacheOptions();
        options(tokenCacheOptions);

        AddTokenCache(services, tokenCacheOptions);

    }
    public static void AddTokenCache(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:TokenCache")
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        AddTokenCache(services, configuration.GetSection(sectionName).Get<TokenCacheOptions>());
    }

    private static void AddTokenCache(IServiceCollection services, TokenCacheOptions tokenCacheOptions)
    {
        if (tokenCacheOptions is null)
        {
            throw new ConfigurationException("TokenCacheOptions is not defined in the configuration file.");
        }

        if (String.IsNullOrWhiteSpace(tokenCacheOptions.CacheName))
        {
            throw new ConfigurationException("TokenCacheOptions.CacheName is not defined in the configuration file.");
        }

        if (TimeSpan.Zero == tokenCacheOptions.MaxTime)
        {
            throw new ConfigurationException("TokenCacheOptions.MaxTime is not defined in the configuration file.");
        }

        services.Configure<TokenCacheOptions>(options =>
        {
            options.CacheName = tokenCacheOptions.CacheName;
            options.MaxTime = tokenCacheOptions.MaxTime;
        });
    }
}

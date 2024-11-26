using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class TokenCacheExtension
{
    public static void AddTokenCache(this IServiceCollection services, Action<TokenCacheOptions> options)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
#endif
        var tokenCacheOptions = new TokenCacheOptions();
        options(tokenCacheOptions);

        AddTokenCache(services, tokenCacheOptions);

    }
    public static void AddTokenCache(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:TokenCache")
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionName);
#else
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }
#endif
        AddTokenCache(services,
                      configuration.GetSection(sectionName)?.Get<TokenCacheOptions>() ?? throw new InvalidOperationException($"Section {sectionName} is not a valid one."));
    }

    private static void AddTokenCache(IServiceCollection services, TokenCacheOptions tokenCacheOptions)
    {
        if (tokenCacheOptions is null)
        {
            throw new ConfigurationException("TokenCacheOptions is not defined in the configuration file.");
        }

        if (string.IsNullOrWhiteSpace(tokenCacheOptions.CacheName))
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

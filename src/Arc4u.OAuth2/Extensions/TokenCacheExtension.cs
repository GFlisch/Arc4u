using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Extensions;
public static class TokenCacheExtension
{
    public static void AddTokenCache(this IServiceCollection services, Action<TokenCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var tokenCacheOptions = new TokenCacheOptions();
        options(tokenCacheOptions);

        AddTokenCache(services, tokenCacheOptions);

    }
    public static void AddTokenCache(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:TokenCache")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionName);

        var tokenCacheOptions = new TokenCacheOptions();
        var defaultMaxTime = tokenCacheOptions.MaxTime;

        var section = configuration.GetSection(sectionName);
        if (section.Exists())
        {
            tokenCacheOptions = section.Get<TokenCacheOptions>() ?? tokenCacheOptions;

            if (!section.GetChildren().Any(c => c.Key == nameof(TokenCacheOptions.MaxTime)))
            {
                tokenCacheOptions.MaxTime = defaultMaxTime;
            }

        }

        AddTokenCache(services, tokenCacheOptions);
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

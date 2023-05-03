using System;
using System.Diagnostics.CodeAnalysis;
using Arc4u.OAuth2.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class ClaimsIdentiferExtension
{
    public static void AddClaimsIdentifier(this IServiceCollection services, Action<ClaimsIdentifierOption> options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        services.Configure<ClaimsIdentifierOption>(options);
    }

#if NET6_0_OR_GREATER
    public static void AddClaimsIdentifier(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:ClaimsIdentifer")
#else
    public static void AddClaimsIdentifier(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:ClaimsIdentifer")
#endif
    {
        AddClaimsIdentifier(services, PrepareAction(configuration, sectionName));
    }

#if NET6_0_OR_GREATER
    public static Action<ClaimsIdentifierOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
#else
    public static Action<ClaimsIdentifierOption> PrepareAction(IConfiguration configuration, string sectionName)
#endif
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        // Standard values used to identify a user.
        var values = new ClaimsIdentifierOption();
        values.AddRange(new[] { "http://schemas.microsoft.com/identity/claims/objectidentifier", "oid" });

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<ClaimsIdentifierOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            values.Clear();
            values.AddRange(option);
        }

        void options(ClaimsIdentifierOption o)
        {
            o.AddRange(values);
        }

        return options;
    }
}

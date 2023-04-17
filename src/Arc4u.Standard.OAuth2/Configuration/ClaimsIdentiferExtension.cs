using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Configuration;
public static class ClaimsIdentiferExtension
{
    public static void AddClaimsIdentifier(this IServiceCollection services, Action<ClaimsIdentifierOption> options)
    {
        services.Configure<ClaimsIdentifierOption>(options);
    }

    public static void AddClaimsIdentifier(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:ClaimsIdentifer")
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

        AddClaimsIdentifier(services, options);
    }
}

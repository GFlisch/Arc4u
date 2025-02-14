using System.Diagnostics.CodeAnalysis;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class ClaimsidentifierExtension
{
    public static void AddClaimsIdentifier(this IServiceCollection services, Action<ClaimsIdentifierOption> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.Configure<ClaimsIdentifierOption>(options);
    }

    public static void AddClaimsIdentifier(this IServiceCollection services, IConfiguration configuration, [DisallowNull] string sectionName = "Authentication:ClaimsIdentifier")
    {
        AddClaimsIdentifier(services, PrepareAction(configuration, sectionName));
    }

    public static Action<ClaimsIdentifierOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        // Standard values used to identify a user.
        var values = new ClaimsIdentifierOption();
        values.AddRange(new[] { ClaimTypes.ObjectIdentifier, ClaimTypes.OID });

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

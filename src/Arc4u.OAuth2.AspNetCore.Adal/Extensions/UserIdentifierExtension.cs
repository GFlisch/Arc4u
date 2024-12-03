using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;

public static class UserIdentifierExtension
{
    public static void AddUserIdentifier(this IServiceCollection services, Action<UserIdentifierOption> options)
    {
        services.Configure<UserIdentifierOption>(options);
    }

    public static void AddUserIdentifier(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:UserIdentifier")
    {
        AddUserIdentifier(services, PrepareAction(configuration, sectionName));
    }

    internal static Action<UserIdentifierOption> PrepareAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        // default value.
        var result = new UserIdentifierOption();

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<UserIdentifierOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            result.Type = option.Type;
        }

        void Options(UserIdentifierOption o)
        {
            o.Type = result.Type;
        }

        return Options;
    }
}

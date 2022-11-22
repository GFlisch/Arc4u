using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Arc4u.Security.Cryptography;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#nullable enable
namespace Arc4u.Extensions
{
    public static class ConfigurationExtension
    {
        private static readonly Regex UserNamePasswordRegex =
            new(@"(?<user>.+?):(?<password>.+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        [MustUseReturnValue]
        public static TConfiguration GetAndDecrypt<TConfiguration>(this IConfiguration configuration, IDecryptionProvider? decryptionProvider) where TConfiguration : new()
        {
            var type = typeof(TConfiguration);
            var config = GetWithoutDecryption<TConfiguration>(configuration);
          
            foreach (var property in
                type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(property => property.PropertyType == typeof(string))
                    .Where(property => property.GetCustomAttribute(typeof(EncryptedAttribute)) != null)
            )
            {
                var encryptedValue = property.GetValue(config) as string ?? string.Empty;
                var decryptedValue = decryptionProvider?.Decrypt(encryptedValue) ?? encryptedValue;
                switch (property.GetCustomAttribute<EncryptedAttribute>()?.FilterValue)
                {
                    case EncryptedAttribute.Filter.Password:
                        decryptedValue = decryptedValue.DeterminePassword();
                        break;
                    case EncryptedAttribute.Filter.Username:
                        decryptedValue = DetermineUsername(decryptedValue);
                        break;
                }

                property.SetValue(config, decryptedValue);
            }

            return config;
        }

        public static string DetermineUsername(this string decryptedValue)
        {
            return UserNamePasswordRegex.Match(decryptedValue).Groups["user"].Value;
        }

        public static string DeterminePassword(this string decryptedValue)
        {
            return UserNamePasswordRegex.Match(decryptedValue).Groups["password"].Value;
        }

        [MustUseReturnValue]
        public static TConfiguration GetWithoutDecryption<TConfiguration>(this IConfiguration configuration) where TConfiguration : new()
        {
            var type = typeof(TConfiguration);

            var config = configuration.GetSection(type.Name).Get<TConfiguration>(options => { options.BindNonPublicProperties = true; });
            if (config is null)
            {
              //  ObjectResolver.Resolve<ILogger<IConfiguration>>().LogError($"The configuration for {type.Name} is not present in the configuration. Using def");
                config = new TConfiguration();
            }

            return config;
        }
    }
}
#nullable restore
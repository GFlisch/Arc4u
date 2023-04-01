using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

public static class SecretDecryptorConfiguratorExtensions
{
    public static IConfigurationBuilder AddCertificateDecryptorConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new SecretCertificateConfigurationSource(new SecretCertificateOptions()));
        return configurationBuilder;
    }

    public static IConfigurationBuilder AddCertificateDecryptorConfiguration(this IConfigurationBuilder configurationBuilder, Action<SecretCertificateOptions> options)
    {
        var config = new SecretCertificateOptions();
        options(config);

        configurationBuilder.Add(new SecretCertificateConfigurationSource(config));
        return configurationBuilder;
    }


    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the previous existing providers and decrypt if needed.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new SecretRijndaelConfigurationSource());
        return configurationBuilder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the previous existing providers and decrypt if needed.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="prefix">Optional prefix used to detect the secret to decrypt. If null, the <see cref="SecretRijndaelConfigurationSource"/> default value.</param>
    /// <param name="secretSectionName">Optional section to read the configuration from the config. If null, the <see cref="SecretRijndaelConfigurationSource"/> default value.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder, string? prefix, string? secretSectionName)
    {
        configurationBuilder.Add(new SecretRijndaelConfigurationSource(prefix, secretSectionName));
        return configurationBuilder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the previous existing providers and decrypt if needed.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="rijndaelConfig">The <see cref="RijndaelConfig"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder, RijndaelConfig rijndaelConfig)
    {
        configurationBuilder.Add(new SecretRijndaelConfigurationSource(rijndaelConfig));
        return configurationBuilder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the previous existing providers and decrypt if needed.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="rijndaelConfig">The <see cref="rijndaelConfig"/> to use.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <returns></returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder, string prefix, RijndaelConfig rijndaelConfig)
    {
        configurationBuilder.Add(new SecretRijndaelConfigurationSource(prefix, rijndaelConfig));
        return configurationBuilder;
    }
}

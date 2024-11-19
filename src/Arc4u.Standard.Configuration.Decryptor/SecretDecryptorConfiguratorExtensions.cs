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
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values based on the default values defined in the <see cref="SecretRijndaelOptions"/>.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Add(new SecretRijndaelConfigurationSource(new SecretRijndaelOptions()));
        return configurationBuilder;
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values based on the values defined in the <see cref="SecretRijndaelOptions"/>.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="options">The <see cref="SecretRijndaelOptions"/> parameters.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddRijndaelDecryptorConfiguration(this IConfigurationBuilder configurationBuilder, Action<SecretRijndaelOptions> options)
    {
        var config = new SecretRijndaelOptions();
        options(config);

        configurationBuilder.Add(new SecretRijndaelConfigurationSource(config));
        return configurationBuilder;
    }
}

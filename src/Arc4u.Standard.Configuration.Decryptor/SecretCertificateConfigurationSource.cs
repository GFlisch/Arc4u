using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Represent the <see cref="IConfigurationProvider"/> collection as an <see cref="IConfigurationSource"/> for decryption based on an <see cref="X509Certificate2"/>.
/// </summary>
public class SecretCertificateConfigurationSource : IConfigurationSource
{
    public const string PrefixDefault = "Encrypt:";
    public const string SecretSectionNameDefault = "EncryptionCertificate";

    /// <summary>
    /// A prefix used to detect encrypted values in a configuration.
    /// </summary>
    private string Prefix { get; init; }

    /// <summary>
    /// A section define in a configuration provider containing the <see cref="CertificateInfo"/> keys.
    /// </summary>
    private string SecretSectionName { get; init; }

    /// <summary>
    /// A certificate to use instead of reading this from a configuration provider.
    /// </summary>
    public X509Certificate2? Certificate { get; set; } = null;

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> with the default prefix <see cref="PrefixDefault" and section <see cref="SecretSectionNameDefault"/>./>
    /// </summary>
    public SecretCertificateConfigurationSource()
    {
        Prefix = PrefixDefault;
        SecretSectionName = SecretSectionNameDefault;
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The certificate is fetched based on the configuration with the respect of the <see cref="CertificateInfo"/>.
    /// </summary>
    /// <param name="prefix">The prefix to use, if null the <see cref="PrefixDefault"/> is used.</param>
    /// <param name="secretSectionName">The section name to use, if null the <see cref="SecretSectionNameDefault"/> is used.</param>
    /// </summary>
    public SecretCertificateConfigurationSource(string? prefix, string? secretSectionName)
    {
        Prefix = prefix ?? PrefixDefault;
        SecretSectionName = secretSectionName ?? SecretSectionNameDefault;
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The prefix use is the default <see cref="PrefixDefault"/>.
    /// </summary>
    /// <param name="certificate">The certificate to use for decription</param>
    public SecretCertificateConfigurationSource(X509Certificate2 certificate)
    {
        Prefix = PrefixDefault;
        Certificate = certificate;

        // will be not used.
        SecretSectionName = SecretSectionNameDefault;      
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// </summary>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="certificate">The certificate to use for decription</param>
    public SecretCertificateConfigurationSource(string prefix, X509Certificate2 certificate)
    {
        Prefix = prefix;
        Certificate = certificate;

        // will be not used.
        SecretSectionName = SecretSectionNameDefault;
    }

    /// <summary>
    /// Builds the <see cref="EnvironmentVariablesConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="EnvironmentVariablesConfigurationProvider"/></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretConfigurationCertificateProvider(Prefix, SecretSectionName, Certificate, builder.Build());
    }
}

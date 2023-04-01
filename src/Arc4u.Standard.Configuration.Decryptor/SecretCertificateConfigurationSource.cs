using System.Security.Cryptography.X509Certificates;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Represent the <see cref="IConfigurationProvider"/> collection as an <see cref="IConfigurationSource"/> for decryption based on an <see cref="X509Certificate2"/>.
/// </summary>
public class SecretCertificateConfigurationSource : IConfigurationSource
{
    public const string PrefixDefault = "Decrypt:";
    public const string SecretSectionNameDefault = "EncryptionCertificate";

    public SecretCertificateConfigurationSource(SecretCertificateOptions options)
    {
        _options = new SecretCertificateOptions
        {
            Prefix = options.Prefix ?? PrefixDefault,
            SecretSectionName = options.SecretSectionName ?? SecretSectionNameDefault,
            CertificateLoader = options.CertificateLoader ?? (options.Certificate is null ? new X509CertificateLoader(null) : null),
            Certificate = options.Certificate,
        };
    }

    private readonly SecretCertificateOptions _options;

    /// <summary>
    /// Builds the <see cref="EnvironmentVariablesConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="EnvironmentVariablesConfigurationProvider"/></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {

        return new SecretConfigurationCertificateProvider(_options, builder.Build());
    }
}

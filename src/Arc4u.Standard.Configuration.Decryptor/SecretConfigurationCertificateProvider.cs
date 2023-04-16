using System.Diagnostics.CodeAnalysis;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Loads configuration key/values from the providers define before this one.
/// Decrypt the value with this one if value starts with the prefix.
/// </summary>
public class SecretConfigurationCertificateProvider : ConfigurationProvider
{
    /// <summary>
    /// Create a <see cref="IConfigurationProvider"/> with a source based on the previous list of providers in the <see cref="IConfigurationRoot"/>.
    /// </summary>
    /// <param name="prefix">Use to identify the values where a decription is needed.</param>
    /// <param name="secretSectionName">Is used to identify the section, coming from the previous providers defined, to read the configuration needed to identify the certificate.</param>
    /// <param name="certificate">An optional parameter, where the user of the class will inject by itself the certificate to use. In this case the secretSectionName parameter is not considered.</param>
    /// <param name="configurationRoot">The <see cref="IConfigurationRoot"/>.</param>
    public SecretConfigurationCertificateProvider([DisallowNull] SecretCertificateOptions options, IConfigurationRoot configurationRoot)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _configurationRoot = configurationRoot;
    }

    private readonly SecretCertificateOptions _options;
    private readonly IConfigurationRoot _configurationRoot;

    /// <summary>
    /// The Load method does the different steps.
    /// - Check if a configuration section exists defining certificate to use.
    /// - Fetch the certificate in the store of the machine.
    /// - Parse the dictionary of item from the providers, and if one value starts with the profix keyword, decrypt it.
    /// - Add the decrypted data to the Data dictionary. So the final Configuration will contains the decrypted value.
    /// - No exception are catched by purpose. So the eror is visible on any logging system or console.
    /// </summary>
    public override void Load()
    {
        Dictionary<string, string?> data = new(StringComparer.OrdinalIgnoreCase);

        var tempRoot = new ConfigurationRoot(new List<IConfigurationProvider>(_configurationRoot.Providers));

        _options.Certificate ??= _options.CertificateLoader?.FindCertificate(tempRoot, _options.SecretSectionName);

        if (_options.Certificate is null || _options.Prefix is null)
        {
            Data = data;
            return;
        }

        // Parse the temproot Data collection of each provider
        foreach (var item in tempRoot.AsEnumerable())
        {
            if (item.Value is not null && item.Value.StartsWith(_options.Prefix))
            {
                var cypher = item.Value.Substring(_options.Prefix.Length);

                data.Add(item.Key, _options.Certificate.Decrypt(cypher));
            }
        }

        Data = data;
    }

}

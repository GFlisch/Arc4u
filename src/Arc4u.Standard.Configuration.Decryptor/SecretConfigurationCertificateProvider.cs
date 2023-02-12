using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.Configuration.Decryptor;

public class SecretConfigurationCertificateProvider : ConfigurationProvider
{
    public SecretConfigurationCertificateProvider(string prefix, string secretSectionName, X509Certificate2? certificate, IConfigurationRoot configurationRoot)
    {
        _configurationRoot = configurationRoot;
        _prefix = prefix;
        _secretSectionName = secretSectionName;
        _certificate = certificate;
    }

    private readonly string _prefix;
    private readonly string _secretSectionName;
    private X509Certificate2? _certificate;

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
        Dictionary<string, string> data = new(StringComparer.OrdinalIgnoreCase);

        var tempRoot = new ConfigurationRoot(new List<IConfigurationProvider>(_configurationRoot.Providers));

        if (_certificate is null)
        {
            var certificate = tempRoot.GetSection(_secretSectionName).Get<CertificateInfo>();

            // For this configuration, no decryption exists. Simply skip this provider.
            if (certificate is null)
            {
                Data = data;
                return;
            }

            _certificate = Certificate.FindCertificate(certificate.Name, certificate.FindType, certificate.Location, certificate.StoreName);

        }

        // Parse the temproot Data collection of each provider
        foreach (var item in tempRoot.AsEnumerable())
        {
            if (item.Value is not null && item.Value.StartsWith(_prefix))
            {
                var cypher = item.Value.Substring(_prefix.Length);

                data.Add(item.Key, Certificate.Decrypt(cypher, _certificate));
            }
        }

        Data = data;
    }


}

using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Loads configuration key/values from the providers define before this one.
/// Decrypt the value with this one if value starts with the prefix.
/// </summary>
public class SecretRijndaelConfigurationProvider : ConfigurationProvider
{
    /// <summary>
    /// Create a <see cref="IConfigurationProvider"/> with a source based on the previous list of providers in the <see cref="IConfigurationRoot"/>.
    /// </summary>
    /// <param name="prefix">Use to identify the values where a decription is needed.</param>
    /// <param name="secretSectionName">Is used to identify the section, coming from the previous providers defined, to read the configuration needed to identify the certificate.</param>
    /// <param name="RijndaelConfig">An optional parameter, where the user of the class will inject by itself the Rijndael key and IV to use. In this case the secretSectionName parameter is not considered.</param>
    /// <param name="configurationRoot">The <see cref="IConfigurationRoot"/>.</param>
    public SecretRijndaelConfigurationProvider(SecretRijndaelOptions options, IList<IConfigurationSource> sources)
    {
        ArgumentNullException.ThrowIfNull(options);

        _rijndaelOptions = options;
        _sources = sources;
    }

    private readonly IList<IConfigurationSource> _sources;
    private readonly SecretRijndaelOptions _rijndaelOptions;

    /// <summary>
    /// The Load method does the different steps.
    /// - Check if a configuration section exists defining the key/iv to use.
    /// - Fetch the key/iv in the configuration providers.
    /// - Parse the dictionary of item from the providers, and if one value starts with the profix keyword, decrypt it.
    /// - Add the decrypted data to the Data dictionary. So the final Configuration will contains the decrypted value.
    /// - No exception are catched by purpose. So the eror is visible on any logging system or console.
    /// </summary>
    public override void Load()
    {
        Dictionary<string, string?> data = new(StringComparer.OrdinalIgnoreCase);

        var tempBuilder = new ConfigurationBuilder();
        foreach (var source in _sources)
        {
            tempBuilder.Add(source);
        }
        var tempRoot = tempBuilder.Build();

        if (_rijndaelOptions.RijnDael is null)
        {
            _rijndaelOptions.RijnDael = tempRoot.GetSection(_rijndaelOptions.SecretSectionName).Get<CypherCodecConfig>();

            // For this configuration, no decryption exists. Simply skip this provider.
            if (_rijndaelOptions.RijnDael is null)
            {
                Data = data;
                return;
            }
        }

        var rijndaelkeys = SecretRijndaelConfigurationProvider.ConvertTo(_rijndaelOptions.RijnDael);

        // Parse the temproot Data collection of each provider
        foreach (var item in tempRoot.AsEnumerable())
        {
            if (item.Value is not null && item.Value.StartsWith(_rijndaelOptions.Prefix))
            {
                var cypher = item.Value.Substring(_rijndaelOptions.Prefix.Length);

                data.Add(item.Key, CypherCodec.DecodeCypherString(cypher, rijndaelkeys.Item1, rijndaelkeys.Item2));
            }
        }

        Data = data;
    }

    private static (byte[], byte[]) ConvertTo(CypherCodecConfig rijndaelConfig)
    {
        return (Convert.FromBase64String(rijndaelConfig.Key), Convert.FromBase64String(rijndaelConfig.IV));
    }
}


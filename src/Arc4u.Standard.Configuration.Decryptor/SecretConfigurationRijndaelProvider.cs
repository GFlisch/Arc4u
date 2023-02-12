using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

public class SecretConfigurationRijndaelProvider : ConfigurationProvider
{
    public SecretConfigurationRijndaelProvider(string prefix, string secretSectionName, RijndaelConfig? rijndaelConfig, IConfigurationRoot configurationRoot)
    {
        _configurationRoot = configurationRoot;
        _prefix = prefix;
        _secretSectionName = secretSectionName;
        _rijndaelConfig = rijndaelConfig;
    }

    private readonly string _prefix;
    private readonly string _secretSectionName;
    private RijndaelConfig? _rijndaelConfig;

    private readonly IConfigurationRoot _configurationRoot;

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
        Dictionary<string, string> data = new(StringComparer.OrdinalIgnoreCase);

        var tempRoot = new ConfigurationRoot(new List<IConfigurationProvider>(_configurationRoot.Providers));

        if (_rijndaelConfig is null)
        {
            _rijndaelConfig = tempRoot.GetSection(_secretSectionName).Get<RijndaelConfig>();

            // For this configuration, no decryption exists. Simply skip this provider.
            if (_rijndaelConfig is null)
            {
                Data = data;
                return;
            }
        }

        var rijndaelkeys = ConvertTo(_rijndaelConfig);

        // Parse the temproot Data collection of each provider
        foreach (var item in tempRoot.AsEnumerable())
        {
            if (item.Value is not null && item.Value.StartsWith(_prefix))
            {
                var cypher = item.Value.Substring(_prefix.Length);

                data.Add(item.Key, Rijndael.DecodeCypherString(cypher, rijndaelkeys.Item1, rijndaelkeys.Item2));
            }
        }

        Data = data;
    }

    private (byte[], byte[]) ConvertTo(RijndaelConfig rijndaelConfig)
    {
        return ( Convert.FromBase64String(rijndaelConfig.Key), Convert.FromBase64String(rijndaelConfig.IV) );
    }
}


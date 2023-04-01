using Arc4u.Security;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Represent the <see cref="IConfigurationProvider"/> collection as an <see cref="IConfigurationSource"/> for decryption based on the Rijndael algorythm.
/// </summary>
public class SecretRijndaelConfigurationSource : IConfigurationSource
{
    public const string PrefixDefault = "Decrypt:";
    public const string SecretSectionNameDefault = "EncryptionRijndael";

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The Rijndael configuration is fetched from the previous providers.
    /// </summary>
    /// <param name="prefix">The prefix to use, if null the <see cref="PrefixDefault"/> is used.</param>
    /// <param name="secretSectionName">The section name to use, if null the <see cref="SecretSectionNameDefault"/> is used.</param>
    /// </summary>
    public SecretRijndaelConfigurationSource(SecretRijndaelOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

         _options = new SecretRijndaelOptions
        {
            Prefix = options.Prefix ?? PrefixDefault,
            SecretSectionName = options.SecretSectionName ?? SecretSectionNameDefault,
            RijnDael = options.RijnDael,
        };
    }

    private readonly SecretRijndaelOptions _options;

    /// <summary>
    /// Builds the <see cref="SecretRijndaelConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="SecretRijndaelConfigurationProvider"/></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretRijndaelConfigurationProvider(_options, builder.Build());
    }
}

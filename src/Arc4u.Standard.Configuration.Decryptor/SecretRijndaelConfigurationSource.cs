using Arc4u.Security;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Decryptor;

/// <summary>
/// Represent the <see cref="IConfigurationProvider"/> collection as an <see cref="IConfigurationSource"/> for decryption based on the Rijndael algorythm.
/// </summary>
public class SecretRijndaelConfigurationSource : IConfigurationSource
{
    public const string PrefixDefault = "Encrypt:";
    public const string SecretSectionNameDefault = "EncryptionRijndael";

    /// <summary>
    /// A prefix used to detect encrypted values in a configuration.
    /// </summary>
    private string Prefix { get; init; }

    /// <summary>
    /// A section define in a configuration provider containing the <see cref="RijndaelConfig"/> keys.
    /// </summary>
    private string SecretSectionName { get; init; }

    /// <summary>
    /// A Rijndael configuration to use instead of reading this from a configuration provider.
    /// </summary>
    public RijndaelConfig? RijndaelConfiguration { get; set; } = null;

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The prefix use is the default <see cref="PrefixDefault"/> and the default section <see cref="SecretSectionNameDefault"/>.
    /// The Rijndael configuration is fetched from the previous providers.
    /// </summary>
    public SecretRijndaelConfigurationSource()
    {
        Prefix = PrefixDefault;
        SecretSectionName = SecretSectionNameDefault;
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The Rijndael configuration is fetched from the previous providers.
    /// </summary>
    /// <param name="prefix">The prefix to use, if null the <see cref="PrefixDefault"/> is used.</param>
    /// <param name="secretSectionName">The section name to use, if null the <see cref="SecretSectionNameDefault"/> is used.</param>
    /// </summary>
    public SecretRijndaelConfigurationSource(string? prefix, string? secretSectionName)
    {
        Prefix = prefix ?? PrefixDefault;
        SecretSectionName = secretSectionName ?? SecretSectionNameDefault;
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.
    /// The prefix use is the default <see cref="PrefixDefault"/>.
    /// </summary>
    /// <param name="rijndaelConfig">The <see cref="RijndaelConfig"/> to use for the key and IV parameters.</param>
    public SecretRijndaelConfigurationSource(RijndaelConfig rijndaelConfig)
    {
        Prefix = PrefixDefault;
        RijndaelConfiguration = rijndaelConfig;

        // not used in this case.
        SecretSectionName = String.Empty;
    }

    /// <summary>
    /// Create a <see cref="IConfigurationSource"/> using the defaults.    /// </summary>
    /// <param name="prefix">The prefix to use</param>
    /// <param name="rijndaelConfig">The <see cref="RijndaelConfig"/> to use for the key and IV parameters.</param>
    public SecretRijndaelConfigurationSource(string prefix, RijndaelConfig rijndaelConfig)
    {
        Prefix = prefix;
        RijndaelConfiguration = rijndaelConfig;

        // not used in this case.
        SecretSectionName = String.Empty;
    }

    /// <summary>
    /// Builds the <see cref="SecretConfigurationRijndaelProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="SecretConfigurationRijndaelProvider"/></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretConfigurationRijndaelProvider(Prefix, SecretSectionName, RijndaelConfiguration, builder.Build());
    }
}
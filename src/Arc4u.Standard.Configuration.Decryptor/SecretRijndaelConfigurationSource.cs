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

    public SecretRijndaelConfigurationSource()
    {
        Prefix = PrefixDefault;
        SecretSectionName = SecretSectionNameDefault;
    }

    public SecretRijndaelConfigurationSource(string? prefix, string? secretSectionName)
    {
        Prefix = prefix ?? PrefixDefault;
        SecretSectionName = secretSectionName ?? SecretSectionNameDefault;
    }

    public SecretRijndaelConfigurationSource(RijndaelConfig rijndaelConfig)
    {
        Prefix = PrefixDefault;
        RijndaelConfiguration = rijndaelConfig;

        // not used in this case.
        SecretSectionName = String.Empty; 
    }

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
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Arc4u.Security.Cryptography;

public static class Certificate
{
    /// <summary>
    /// Encrypt a text and return an encrypted version formated in a 64String.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <returns>The encrypted plain text.</returns>
    public static string Encrypt(this X509Certificate2 x509, string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        var bytes = Encoding.UTF8.GetBytes(plainText.Trim());

        try
        {
            if (bytes.Length < 191)
            {
                return x509.Encrypt(bytes);
            }
        }
        catch (Exception)
        {
            // will use the Aes encryption.
            // Encapsulate in case of on an unexpected behaviour on an non tested platform.
        }

        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();

        var encryptedKey = x509.Encrypt(aes.Key);
        var encryptedIV = x509.Encrypt(aes.IV);
        var encryptedData = CypherCodec.EncodeClearText(plainText, aes.Key, aes.IV);

        return $"{encryptedKey}.{encryptedIV}.{encryptedData}";

    }

    /// <summary>
    /// Encrypt a byte array and return an encrypted version formated in a 64String.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <returns>The encrypted plain text.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static string Encrypt(this X509Certificate2 x509, byte[] content)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(content);
#else
        if (null == content)
        {
            throw new ArgumentNullException(nameof(content));
        }
#endif
        byte[]? cipherBytes;

        using (var rsa = x509.GetRSAPublicKey())
        {
            cipherBytes = rsa?.Encrypt(content, RSAEncryptionPadding.OaepSHA256);
        }

        return null == cipherBytes ? string.Empty : Convert.ToBase64String(cipherBytes);
    }

    /// <summary>
    /// Decrypt an formated 64 string encrypted and return the text in clear.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <returns>The decrypted text.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string Decrypt(this X509Certificate2 x509, string base64CypherString)
    {
        if (base64CypherString.Contains('.'))
        {
            string[] parts = base64CypherString.Split('.');
            if (parts.Length != 3)
            {
                throw new ApplicationException("Invalid encrypted string format");
            }

            var key = x509.DecryptStringToBytes(parts[0]);
            var iv = x509.DecryptStringToBytes(parts[1]);

            return CypherCodec.DecodeCypherString(parts[2], key, iv);
        }

        return Encoding.UTF8.GetString(x509.DecryptStringToBytes(base64CypherString));
    }

    /// <summary>
    /// Decrypt an formated 64 string encrypted and return the array of bytes.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <param name="x509">The certificate used to encrypt</param>
    /// <returns>The decrypted byte array.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static byte[] DecryptStringToBytes(this X509Certificate2 x509, string base64CypherString)
    {
        if (string.IsNullOrWhiteSpace(base64CypherString))
        {
            throw new ArgumentNullException(nameof(base64CypherString));
        }

        if (!x509.HasPrivateKey)
        {
            throw new CryptographicException(string.Format(CultureInfo.InvariantCulture, "The certificate {0} has no private key!", x509.FriendlyName));
        }

        var cipherBytes = Convert.FromBase64String(base64CypherString);

        using var rsa = x509.GetRSAPrivateKey();
        return rsa?.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256) ?? [];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;

public static class Certificate
{
    [Obsolete("Use the X509CertificateLoader instead.")]
    public static X509Certificate2 FindCertificate(String find, X509FindType findType = X509FindType.FindBySubjectName, StoreLocation location = StoreLocation.LocalMachine, StoreName name = StoreName.My)
    {
        var certificateStore = new X509Store(name, location);

        try
        {
            certificateStore.Open(OpenFlags.ReadOnly);

            var certificates = certificateStore.Certificates.Find(findType, find, false);

            return certificates.Count > 0 ? certificates[0] : throw new KeyNotFoundException("No certificate found for the given criteria.");
        }
        finally
        {
            certificateStore.Close();
        }
    }

    [Obsolete("Use the X509CertificateLoader instead.")]
    public static X509Certificate2 FindCertificate(IKeyValueSettings settings)
    {
        var certificateName = settings.Values.ContainsKey("CertificateName") ? settings.Values["CertificateName"] : string.Empty;
        var findType = settings.Values.ContainsKey("FindType") ? settings.Values["FindType"] : string.Empty;
        var storeLocation = settings.Values.ContainsKey("StoreLocation") ? settings.Values["StoreLocation"] : string.Empty;
        var storeName = settings.Values.ContainsKey("StoreName") ? settings.Values["StoreName"] : string.Empty;

        if (null == certificateName)
        {
            throw new AppException("No CertificateName key found in the settings provided.");
        }
        else
        {
            var certificateInfo = new CertificateInfo
            {
                Name = certificateName
            };

            if (Enum.TryParse(findType, out X509FindType x509FindType))
            {
                certificateInfo.FindType = x509FindType;
            }
            if (Enum.TryParse(storeLocation, out StoreLocation storeLocation_))
            {
                certificateInfo.Location = storeLocation_;
            }
            if (Enum.TryParse(storeName, out StoreName storeName_))
            {
                certificateInfo.StoreName = storeName_;
            }

            return FindCertificate(certificateInfo);
        }
    }

    [Obsolete("Use the X509CertificateLoader instead.")]
    public static X509Certificate2 FindCertificate(CertificateInfo certificateInfo)
    {
        var certificate = Certificate.FindCertificate(
                                    certificateInfo.Name,
                                    certificateInfo.FindType,
                                    certificateInfo.Location,
                                    certificateInfo.StoreName);

        return certificate;
    }

    [Obsolete("Use the X509CertificateLoader instead.")]
    public static X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName)
    {
        var certificateInfo = configuration.GetSection(sectionName).Get<CertificateInfo>();

        return Certificate.FindCertificate(certificateInfo);
    }

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

        using (var aes = Aes.Create())
        {
            aes.GenerateKey();
            aes.GenerateIV();

            var encryptedKey = x509.Encrypt(aes.Key);
            var encryptedIV = x509.Encrypt(aes.IV);
            var encryptedData = CypherCodec.EncodeClearText(plainText, aes.Key, aes.IV);

            return $"{encryptedKey}.{encryptedIV}.{encryptedData}";
        }

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
        if (null == x509)
        {
            throw new ArgumentNullException(nameof(x509));
        }

        if (null == content)
        {
            throw new ArgumentNullException(nameof(content));
        }

        byte[] cipherBytes = null;

        using (var rsa = x509.GetRSAPublicKey())
        {
            cipherBytes = rsa.Encrypt(content, RSAEncryptionPadding.OaepSHA256);
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

            byte[] key = x509.DecryptStringToBytes(parts[0]);
            byte[] iv = x509.DecryptStringToBytes(parts[1]);

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
        if (null == x509)
        {
            throw new ArgumentNullException(nameof(x509));
        }

        if (string.IsNullOrWhiteSpace(base64CypherString))
        {
            throw new ArgumentNullException(nameof(base64CypherString));
        }

        if (!x509.HasPrivateKey)
        {
            throw new CryptographicException(string.Format("The certificate {0} has no private key!", x509.FriendlyName));
        }

        var cipherBytes = Convert.FromBase64String(base64CypherString);

        using var rsa = x509.GetRSAPrivateKey();
        return rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
    }
}

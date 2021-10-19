using Arc4u.Diagnostics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Arc4u.Security.Cryptography
{
    public static class Certificate
    {
        public static X509Certificate2 FindCertificate(String find, X509FindType findType = X509FindType.FindBySubjectName, StoreLocation location = StoreLocation.LocalMachine, StoreName name = StoreName.My)
        {
            var certificateStore = new X509Store(name, location);

            try
            {
                certificateStore.Open(OpenFlags.ReadOnly);

                var certificates = certificateStore.Certificates.Find(findType, find, false);

                if (certificates.Count > 0)
                    return certificates[0];

                throw new KeyNotFoundException("No certificate found for the given criteria.");
            }
            finally
            {
                certificateStore.Close();
            }
        }

        /// <summary>
        /// Encrypt a text and return an encrypted version formated in a 64String.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <param name="x509">The certificate used to encrypt</param>
        /// <returns>The encrypted plain text.</returns>
        public static String Encrypt(string plainText, X509Certificate2 x509)
        {
            if (null == x509)
                throw new ArgumentNullException("x509");
            if (String.IsNullOrWhiteSpace(plainText))
                throw new ArgumentNullException("plainText");

            var plainBytes = Encoding.UTF8.GetBytes(plainText.Trim());
            byte[] cipherBytes = null;
            using (var rsa = x509.GetRSAPublicKey())
            {
                cipherBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
            }

            return null == cipherBytes ? String.Empty : Convert.ToBase64String(cipherBytes);
        }

        public static String Decrypt(String base64CypherString, X509Certificate2 x509)
        {
            if (null == x509)
                throw new ArgumentNullException("x509");
            if (String.IsNullOrWhiteSpace(base64CypherString))
                throw new ArgumentNullException("base64CypherString");
            if (!x509.HasPrivateKey)
                throw new CryptographicException(String.Format("The certificate {0} has no private key!", x509.FriendlyName));

            var cipherBytes = Convert.FromBase64String(base64CypherString);

            using (var rsa = x509.GetRSAPrivateKey())
            {
                return Encoding.UTF8.GetString(rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256));
            }
        }

        public static X509Certificate2 ExtractCertificate(IKeyValueSettings settings)
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
                    certificateInfo.FindType = x509FindType;
                if (Enum.TryParse(storeLocation, out StoreLocation storeLocation_))
                    certificateInfo.Location = storeLocation_;
                if (Enum.TryParse(storeName, out StoreName storeName_))
                    certificateInfo.StoreName = storeName_;

                try
                {
                    var certificate = Certificate.FindCertificate(
                                        certificateInfo.Name,
                                        certificateInfo.FindType,
                                        certificateInfo.Location,
                                        certificateInfo.StoreName);

                    return certificate;
                }
                catch (KeyNotFoundException)
                {
                    Logger.Technical.From(typeof(Certificate)).Error($"No certificate found with {certificateInfo.FindType} =  in location = {certificateInfo.Location}.").Log();
                    throw;
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using Arc4u.Diagnostics;

namespace Arc4u.Security.Cryptography
{
    /// <summary>
    ///     Provides decryption using certificates taken from the Windows certificate store.
    ///     The configuration which certificate to choose has to be provided by <see cref="IKeyValueSettings" />.
    /// </summary>
    public sealed class CertificateDecryptionProvider : IDecryptionProvider
    {
        private readonly IEncryptionSettings _encryptionSettings;

        public CertificateDecryptionProvider(IEncryptionSettings encryptionSettings)
        {
            _encryptionSettings = encryptionSettings ?? throw new ArgumentNullException(nameof(encryptionSettings));
        }
        
        /// <inheritdoc />
        public string Decrypt(string cypher)
        {
            if (string.IsNullOrEmpty(cypher))
            {
                return cypher;
            }
            
            var certificateInfo = GetCertificateInfo();

            try
            {
                var certificate = Certificate.FindCertificate(
                    certificateInfo.Name,
                    certificateInfo.FindType,
                    certificateInfo.Location,
                    certificateInfo.StoreName);

                return Certificate.Decrypt(cypher, certificate);
            }
            catch (KeyNotFoundException)
            {
                LoggerBase.Technical.From(this)
                    .Error($"No certificate found with {certificateInfo.FindType} = {certificateInfo.Name} in location = {certificateInfo.Location}.")
                    .Add("FindType", certificateInfo.FindType.ToString())
                    .Add("Name", certificateInfo.Name)
                    .Add("Location", certificateInfo.Location.ToString())
                    .Log();
                throw;
            }
        }

        private CertificateInfo GetCertificateInfo()
        {
            var certificateInfo = new CertificateInfo
            {
                Name = _encryptionSettings.CertificateName
            };

            if (_encryptionSettings.FindType.HasValue)
            {
                certificateInfo.FindType = _encryptionSettings.FindType.Value;
            }
            
            if (_encryptionSettings.StoreLocation.HasValue)
            {
                certificateInfo.Location = _encryptionSettings.StoreLocation.Value;
            }
            
            if (_encryptionSettings.StoreName.HasValue)
            {
                certificateInfo.StoreName = _encryptionSettings.StoreName.Value;
            }

            return certificateInfo;
        }
    }
}
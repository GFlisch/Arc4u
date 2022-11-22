using System.Security.Cryptography.X509Certificates;
#nullable enable
namespace Arc4u.Security.Cryptography
{
    public class DecryptionConfiguration : IEncryptionSettings
    {
        public string? CertificateName { get; set; }
        public X509FindType? FindType { get; set; }
        public StoreName? StoreName { get; set; }
        public StoreLocation? StoreLocation { get; set; }

        public bool IsConfigured => !string.IsNullOrEmpty(CertificateName) && FindType is not null && StoreName is not null && StoreLocation is not null;
    }
}
#nullable restore
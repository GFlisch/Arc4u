using System.Security.Cryptography.X509Certificates;
#nullable enable
namespace Arc4u.Security.Cryptography
{
    public interface IEncryptionSettings
    {
        string? CertificateName { get; }

        X509FindType? FindType { get; }

        StoreLocation? StoreLocation { get; }

        StoreName? StoreName { get; }

        bool IsConfigured { get; }
    }
}
#nullable restore
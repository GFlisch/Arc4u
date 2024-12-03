using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;
public interface IX509CertificateLoader
{
    public X509Certificate2 FindCertificate(CertificateInfo certificateInfo);

    public X509Certificate2 FindCertificate(CertificateFilePathInfo? certificateInfo);

    public X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName);
}

public static class IX509CertificateLoaderExtensionMethods
{
    public static X509Certificate2? FindCertificate(this IX509CertificateLoader x509CertificateLoader, CertificateStoreOrFileInfo? certificateInfo)
    {
        // For this configuration, no decryption exists. Simply skip this provider.
        if (certificateInfo is null)
        {
            return null;
        }

        if (certificateInfo.Store is not null)
        {
            return x509CertificateLoader.FindCertificate(certificateInfo.Store);
        }
        else
        {
            if (certificateInfo.File is null)
            {
                throw new InvalidOperationException("No certificate information found in the configuration.");
            }
            return x509CertificateLoader.FindCertificate(certificateInfo.File);
        }
    }
}

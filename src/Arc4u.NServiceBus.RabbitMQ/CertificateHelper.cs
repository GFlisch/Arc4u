using System.Security.Cryptography.X509Certificates;
using Arc4u.Security;
using Arc4u.Security.Cryptography;
using NServiceBus;

namespace Arc4u.NServiceBus.RabbitMQ;

public static class CertificateHelper
{
    public static void SetCertificateAuthentication(this TransportExtensions<RabbitMQTransport> transport, IKeyValueSettings settings)
    {
        transport.UseExternalAuthMechanism(); // to avoid username password

        transport.SetClientCertificate(FindCertificate(settings));
    }

    private static X509Certificate2 FindCertificate(IKeyValueSettings settings)
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

            var x509Loader = new X509CertificateLoader(null);

            return x509Loader.FindCertificate(certificateInfo);
        }
    }
}

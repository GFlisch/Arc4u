using Arc4u.Security.Cryptography;
using NServiceBus;

namespace Arc4u.NServiceBus.RabbitMQ
{
    public static class CertificateHelper
    {
        public static void SetCertificateAuthentication(this TransportExtensions<RabbitMQTransport> transport, Arc4u.IKeyValueSettings settings)
        {
            transport.UseExternalAuthMechanism(); // to avoid username password

            var certificate = Certificate.ExtractCertificate(settings);

            transport.SetClientCertificate(certificate);
        }


    }

}

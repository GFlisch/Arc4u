using System.Security.Cryptography.X509Certificates;

namespace Arc4u.gRPC.ChannelCertificate;

public interface IRootCertificateExtractor
{
    X509Certificate2? FetchCertificateFor(Uri rootUrl);
}

using System;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.gRPC.ChannelCertificate
{
    public interface IRootCertificateExtractor
    {
        X509Certificate2 Certificate { get; }

        bool FetchCertificateFor(Uri rootUrl);
    }
}

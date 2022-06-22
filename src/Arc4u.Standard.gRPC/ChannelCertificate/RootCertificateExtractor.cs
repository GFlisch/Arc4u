using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Arc4u.gRPC.ChannelCertificate
{
    [Export(typeof(IRootCertificateExtractor))]
    public class RootCertificateExtractor : IRootCertificateExtractor
    {
        public RootCertificateExtractor(ILogger<RootCertificateExtractor> logger)
        {
            _logger = logger;
        }

        readonly ILogger<RootCertificateExtractor> _logger;

        public X509Certificate2 Certificate { get; private set; }

        public bool FetchCertificateFor(Uri rootUrl)
        {
            Certificate = null;

            if (!rootUrl.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Secure https protocol is expected.");

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(rootUrl);
                request.AllowAutoRedirect = false;
                request.ServerCertificateValidationCallback = ServerCertificateValidationCallback;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                _logger.Technical().LogException(ex);

                return false;
            }

            return true;
        }

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Certificate = new X509Certificate2(certificate);

            _logger.Technical().System($"Certificate callback received with Subject = {certificate.Subject}.").Log();

            return SslPolicyErrors.None == sslPolicyErrors;
        }
    }
}

using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Arc4u.gRPC.ChannelCertificate
{
    [Export, Shared]
    public class RootPemCertificates
    {
        public RootPemCertificates(IRootCertificateExtractor certificateExtractor, ILogger<RootPemCertificates> logger)
        {
            _certificateExtractor = certificateExtractor;
            _certificateExtractor = certificateExtractor;

            PemsCollections = new Dictionary<string, string>();
            _logger = logger;
        }

        readonly IRootCertificateExtractor _certificateExtractor;
        readonly ILogger<RootPemCertificates> _logger;
        static object _lock = new object();

        public IReadOnlyDictionary<string, string> Pems { get => new ReadOnlyDictionary<string, string>(PemsCollections); }

        private Dictionary<string, string> PemsCollections;

        public string GetPemFor(Uri rootUri)
        {
            if (null == rootUri)
                throw new ArgumentNullException(nameof(rootUri));

            if (PemsCollections.ContainsKey(rootUri.Host)) return PemsCollections[rootUri.Host];

            try
            {
                lock (_lock)
                {
                    if (!_certificateExtractor.FetchCertificateFor(rootUri))
                        throw new KeyNotFoundException(rootUri.ToString());

                    if (null == _certificateExtractor.Certificate)
                    {
                        _logger.Technical().System($"Certificate was not retrieved from the uri.").Log();
                        throw new KeyNotFoundException(rootUri.ToString());
                    }

                    _logger.Technical().System($"Certificate used is {_certificateExtractor.Certificate.Subject}.").Log();

                    var pem = ExportToPem(_certificateExtractor.Certificate);

                    if (string.IsNullOrWhiteSpace(pem))
                    {
                        _logger.Technical().System($"Pem extaction is empty.").Log();
                        throw new KeyNotFoundException(rootUri.ToString());
                    }

                    PemsCollections.Add(rootUri.Host, pem);

                    return pem;
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();

                throw new KeyNotFoundException(rootUri.ToString());
            }

        }


        /// <summary>
        /// Export a certificate to a PEM format string
        /// </summary>
        /// <param name="cert">The certificate to export</param>
        /// <returns>A PEM encoded string</returns>
        public static string ExportToPem(X509Certificate2 cert)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                builder.AppendLine("-----END CERTIFICATE-----");

            }
            catch (Exception)
            {
            }

            return builder.ToString();
        }
    }
}

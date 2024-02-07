using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.gRPC.ChannelCertificate;

[Export, Shared]
public class RootPemCertificates
{
    public RootPemCertificates(IRootCertificateExtractor certificateExtractor, ILogger<RootPemCertificates> logger)
    {
        _certificateExtractor = certificateExtractor;
        _pemsCollections = new Dictionary<string, string>();
        _logger = logger;
    }

    readonly IRootCertificateExtractor _certificateExtractor;
    readonly ILogger<RootPemCertificates> _logger;
    static readonly object _lock = new();

    public IReadOnlyDictionary<string, string> Pems => _pemsCollections;

    private readonly Dictionary<string, string> _pemsCollections;

    public string GetPemFor(Uri rootUri)
    {
        if (rootUri is null)
        {
            throw new ArgumentNullException(nameof(rootUri));
        }

        if (_pemsCollections.TryGetValue(rootUri.Host, out var pem))
        {
            return pem;
        }

        try
        {
            var certificate = _certificateExtractor.FetchCertificateFor(rootUri);

            if (certificate is null)
            {
                _logger.Technical().System($"Certificate was not retrieved from the uri.").Log();
                throw new KeyNotFoundException(rootUri.ToString());
            }

            _logger.Technical().System($"Certificate used is {certificate.Subject}.").Log();

            pem = ExportToPem(certificate);

            if (string.IsNullOrWhiteSpace(pem))
            {
                _logger.Technical().System($"Pem extaction is empty.").Log();
                throw new KeyNotFoundException(rootUri.ToString());
            }

            lock (_lock)
            {
                _pemsCollections.Add(rootUri.Host, pem);
            }

            return pem;
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
        var builder = new StringBuilder();

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

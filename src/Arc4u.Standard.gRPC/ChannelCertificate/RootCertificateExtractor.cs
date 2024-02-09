#nullable enable
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.gRPC.ChannelCertificate;

[Export(typeof(IRootCertificateExtractor))]
public class RootCertificateExtractor : IRootCertificateExtractor, IDisposable
{
    public RootCertificateExtractor(ILogger<RootCertificateExtractor> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, ServerCertificateCustomValidationCallback = ServerCertificateValidationCallback });
    }

    private readonly ILogger<RootCertificateExtractor> _logger;
    private readonly HttpClient _httpClient;
#if NETSTANDARD
    private static readonly string _key = nameof(CertificateHolder);
#else
    private static readonly HttpRequestOptionsKey<CertificateHolder> _key = new(nameof(CertificateHolder));
#endif

    private sealed class CertificateHolder
    {
        public X509Certificate2? Certificate { get; set; }
    }

    public X509Certificate2? FetchCertificateFor(Uri rootUrl)
    {
        if (!rootUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Secure https protocol is expected.");
        }

        try
        {
            var certificateHolder = new CertificateHolder();
            using var request = new HttpRequestMessage(HttpMethod.Get, rootUrl);
#if NETSTANDARD
            request.Properties[_key] = certificateHolder;
            using var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
#else
            request.Options.Set(_key, certificateHolder);
            using var response = _httpClient.Send(request);
#endif
            // Note that this will oonly work once: next calls will not trigger ServerCertificateCustomValidationCallback because _httpClient will cache the response.
            return certificateHolder.Certificate;
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
            return null;
        }
    }

    private bool ServerCertificateValidationCallback(HttpRequestMessage sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (certificate is not null)
        {
            _logger.Technical().System($"Certificate callback received with Subject = {certificate.Subject}.").Log();
#if NETSTANDARD
            if (sender.Properties.TryGetValue(_key, out var certificateHolder))
            {
                ((CertificateHolder)certificateHolder).Certificate = new X509Certificate2(certificate);
            }
#else
            if (sender.Options.TryGetValue(_key, out var certificateHolder))
            {
                certificateHolder.Certificate = new X509Certificate2(certificate);
            }
#endif
        }
        else
        {
            _logger.Technical().System($"Certificate callback received no certificate.").Log();
        }

        return sslPolicyErrors == SslPolicyErrors.None;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

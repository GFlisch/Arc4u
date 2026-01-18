#if NET10_0_OR_GREATER

using System.Security.Cryptography.X509Certificates;
using Arc4u.Security.Cryptography;
using Arc4u.Security.CustomRootCA;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Extensions;

public static class CustomRootCaExtension
{
    extension(IHttpClientBuilder builder)
    {
        public IHttpClientBuilder ConfigureLocalCaCertificate(string? certificateOptionKey = null)
        {
            return builder.ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler();
                var logger = sp.GetRequiredService<ILogger<HttpClientHandler>>();
                var x509Loader = sp.GetRequiredService<IX509CertificateLoader>();

                var options = new List<CARootOption>();
                var caRootOptions = sp.GetRequiredService<IOptionsMonitor<CARootOption>>();
                if (!string.IsNullOrWhiteSpace(certificateOptionKey))
                {
                    options.Add(caRootOptions.Get(certificateOptionKey));
                }
                else
                {
                    options.AddRange(CustomRootCaExtensions.CertificateOptionKeys.Select(key => caRootOptions.Get(key)));
                }

                // Load certificates once during handler creation (OPTIMIZATION)
                var customRootCertificates = new List<X509Certificate2>();
                try
                {
                    foreach (var caOption in options)
                    {
                        var certificate = caOption.GetCertificate(x509Loader);
                        if (certificate is not null)
                        {
                            customRootCertificates.Add(certificate);
                        }
                    }

                    if (customRootCertificates.Count == 0)
                    {
                        logger.LogWarning("No valid CA certificates loaded for custom root trust");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load internal CA certificate from {Certificate}", certificateOptionKey);
                }

                // Only configure callback if we have certificates (OPTIMIZATION)
                if (customRootCertificates.Count > 0)
                {
                    handler.ServerCertificateCustomValidationCallback = (_, cert, chain, sslPolicyErrors) =>
                    {
                        // If no errors, accept immediately
                        if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                            return true;

                        if (chain is null) return false;

                        // Configure chain to use custom root trust
                        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                        foreach (var certificate in customRootCertificates)
                        {
                            chain.ChainPolicy.CustomTrustStore.Add(certificate);
                        }

                        var isValid = chain.Build(cert!);

                        if (!isValid)
                        {
                            logger.LogWarning(
                                "Certificate validation failed for {Subject}. Chain status: {ChainStatus}",
                                cert?.Subject,
                                string.Join(", ", chain.ChainStatus.Select(s => s.StatusInformation)));
                        }

                        return isValid;
                    };
                }

                return handler;
            });
        }
    }
}

#endif

#if NET10_0_OR_GREATER

using System.Security.Cryptography.X509Certificates;
using Arc4u.Configuration;
using Arc4u.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Security.CustomRootCA;

public static class CustomRootCaExtensions
{
    static CustomRootCaExtensions()
    {
        CertificateOptionKeys = [];
    }

    public static List<string> CertificateOptionKeys { get; }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddCustomRootCA(string name, Action<CARootOption> configureOptions)
        {
            var certificate = new CARootOption();
            configureOptions(certificate);

            if (string.IsNullOrWhiteSpace(certificate.CaFilePath) && string.IsNullOrWhiteSpace(certificate.CaPem) && certificate.Store is null)
            {
                throw new ConfigurationException($"The certificate {name} does not have a CA file or a CA PEM or a Store defined.");
            }

            // Check that the certificate has only one CA source.
            var definedSources = 0;
            if (!string.IsNullOrWhiteSpace(certificate.CaFilePath)) definedSources++;
            if (!string.IsNullOrWhiteSpace(certificate.CaPem)) definedSources++;
            if (certificate.Store is not null) definedSources++;
            if (definedSources > 1)
            {
                throw new ConfigurationException($"The certificate {name} has more than one CA source defined.");
            }

            CertificateOptionKeys.Add(name);
            services.Configure(name, configureOptions);
            return services;
        }

        public IServiceCollection AddCustomRootCA(IConfiguration configuration, string sectionName = "CustomRootCA")
        {
            var section = configuration.GetSection(sectionName);
            if (!section.Exists())
            {
                // Do not throw an exception if the section is not found.
                // So we can activate or not the feature depending on the configuration.
                return services;
            }
            // Read the collection of certificates from the configuration section
            var certificates = section.Get<Dictionary<string, CARootOption>>();

            if (certificates is null)
            {
                return services;
            }

            // Check that each certificate has 1 field.
            foreach (var certificate in certificates)
            {
                var option = new CARootOption
                {
                    CaFilePath = certificate.Value.CaFilePath,
                    CaPem = certificate.Value.CaPem,
                    Store = certificate.Value.Store
                };

                services.AddCustomRootCA(certificate.Key, FillCustomRootCa);
                continue;

                void FillCustomRootCa(CARootOption fillerOption)
                {
                    fillerOption.CaFilePath = option.CaFilePath;
                    fillerOption.CaPem = option.CaPem;
                    fillerOption.Store = option.Store;
                }
            }

            return services;
        }
    }

    extension(CARootOption option)
    {
        public X509Certificate2? GetCertificate(IX509CertificateLoader certificateLoader)
        {
            if (!string.IsNullOrWhiteSpace(option.CaPem)) return X509Certificate2.CreateFromPem(option.CaPem);

            if (!string.IsNullOrWhiteSpace(option.CaFilePath)) return X509Certificate2.CreateFromPem(File.ReadAllText(option.CaFilePath));

            return option.Store is not null ? certificateLoader.FindCertificate(option.Store) : null;
        }
    }
}

#endif

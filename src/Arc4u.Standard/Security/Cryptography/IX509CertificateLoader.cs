using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Security.Cryptography;
public interface IX509CertificateLoader
{
    public X509Certificate2 FindCertificate(String find, X509FindType findType = X509FindType.FindBySubjectName, StoreLocation location = StoreLocation.LocalMachine, StoreName name = StoreName.My);

    public X509Certificate2 FindCertificate(CertificateInfo certificateInfo);

#if NET6_0_OR_GREATER
    public X509Certificate2 FindCertificate(CertificateStoreOrFileInfo certificateInfo);
#endif

    public X509Certificate2? FindCertificate(IConfiguration configuration, string sectionName);
}

using System.Security.Cryptography.X509Certificates;

namespace Arc4u.Security
{
    /// <summary>
    /// This class is used to extract from config file information about a certificate.
    /// </summary>
    public class CertificateInfo
    {
        /// <summary>
        /// Name used to search a certificate in the store based on the <see cref="X509FindType"/>.
        /// </summary>
        public String? Name { get; set; }

        /// <summary>
        /// The <see cref="X509FindType"/> of the certificate.
        /// Default value is FindBySubjectName.
        /// </summary>
        public X509FindType FindType { get; set; } = X509FindType.FindBySubjectName;

        /// <summary>
        /// The <see cref="StoreLocation"/> in the certificate store.
        /// </summary>
        public StoreLocation Location { get; set; } = StoreLocation.LocalMachine;

        /// <summary>
        /// The <see cref="StoreName"/> folder.
        /// </summary>
        public StoreName StoreName { get; set; } = StoreName.My;
    }
}

using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class ClientSecretAuthenticationOption
    {
        public ClientSecretAuthenticationOption(IKeyValueSettings settings)
        {
            Settings = settings;
        }

        public IKeyValueSettings Settings { get; set; }

        public String SecretKey { get; set; } = "SecretKey";

        public CertificateOption Certificate { get; set; } = new CertificateOption();

        public class CertificateOption
        {
            public String SecretKey { get; set; } = "CertificateKey";

            public string Location { get; set; }

            public string Name { get; set; }

            public string FindType { get; set; }

            public string StoreName { get; set; } = null;
        }
    }
}

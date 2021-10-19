using System;
using System.Collections.Generic;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class ClaimsPrincipalMiddlewareOption
    {
        public ClaimsFillerOption ClaimsFillerOptions { get; set; } = new ClaimsFillerOption();

        public OpenIdOption OpenIdOptions { get; set; } = new OpenIdOption();

        public String RedirectAuthority { get; set; } = String.Empty;

        public class OpenIdOption
        {
            public IEnumerable<String> ForceAuthenticationForPaths { get; set; } = new List<String>();
        }

        public class ClaimsFillerOption
        {
            public bool LoadClaimsFromClaimsFillerProvider { get; set; } = false;

            public IEnumerable<IKeyValueSettings> Settings { get; set; } = null;
        }
    }
}

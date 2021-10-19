using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Arc4u.Security.Principal
{
    [Export(typeof(IClaimProfileFiller)), Shared]
    public class ClaimsProfileFiller : IClaimProfileFiller
    {
        public UserProfile GetProfile(IIdentity identity)
        {
            if (null == identity)
                throw new ArgumentNullException("identity");

            if (!(identity is ClaimsIdentity))
                throw new NotSupportedException("Only identity from IClaimsIdentity are allowed.");

            var claimsIdentity = (ClaimsIdentity)identity;

            // Create a UserProfile based on the identity received.
            var claimCulture = ExtractClaimValue(claimsIdentity.Claims, IdentityModel.Claims.ClaimTypes.Culture);

            CultureInfo culture;
            try
            {
                culture = new CultureInfo(claimCulture);
            }
            catch (Exception)
            {
                culture = new CultureInfo("en-us");
            }
            var samAccountName = String.Empty;
            var domain = string.Empty;
            var name = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.Name, IdentityModel.Claims.ClaimTypes.Name);
            var surName = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.Surname, IdentityModel.Claims.ClaimTypes.Surname);
            var givenName = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.GivenName, IdentityModel.Claims.ClaimTypes.GivenName);


            var email = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.Email, IdentityModel.Claims.ClaimTypes.Email);
            var upn = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.Upn, IdentityModel.Claims.ClaimTypes.Upn);
            // transform the upn in domain\user.
            if (!String.IsNullOrWhiteSpace(upn))
            {
                if (upn.Contains("@"))
                {
                    var array = upn.Split('@');
                    if (array.Length == 2)
                    {
                        samAccountName = array[0];
                        domain = array[1];
                        if (domain.Contains(".")) domain = domain.Split('.')[0];
                    }
                }
                else
                if (upn.Contains("\\"))
                {
                    var array = upn.Split('\\');
                    if (array.Length == 2)
                    {
                        samAccountName = array[1];
                        domain = array[0];
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(name))
                name = String.IsNullOrWhiteSpace(samAccountName) ? $"{givenName} {surName}" : samAccountName;

            var company = ExtractClaimValue(claimsIdentity.Claims, IdentityModel.Claims.ClaimTypes.Company);
            var phone = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.HomePhone);
            var mobile = ExtractClaimValue(claimsIdentity.Claims, ClaimTypes.MobilePhone);

            var sid = ExtractClaimValue(claimsIdentity.Claims, IdentityModel.Claims.ClaimTypes.Sid);
            sid = String.IsNullOrWhiteSpace(sid) ? "S-1-0-0" : sid;

            return new UserProfile(name,
                                   email,
                                   String.Empty,
                                   company,
                                   givenName,
                                   surName,
                                   sid,
                                   String.Empty,
                                   mobile,
                                   phone,
                                   String.Empty,
                                   String.Empty,
                                   upn,
                                   String.Empty,
                                   String.Empty,
                                   String.Empty,
                                   String.Empty,
                                   samAccountName,
                                   domain,
                                   culture,
                                   String.Empty,
                                   String.Empty);
        }

        private String ExtractClaimValue(IEnumerable<Claim> claims, params string[] claimTypes)
        {
            var claim = claims.FirstOrDefault(c => claimTypes.Any(t => t.Equals(c.Type, StringComparison.InvariantCultureIgnoreCase)));
            try
            {
                if (null != claim)
                    return claim.Value;

                return String.Empty;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

    }
}

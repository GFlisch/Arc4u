using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Arc4u.OAuth2.Token
{
    public static class UserProfileExt
    {
        public static readonly string tokenExpirationClaimType = "exp";

        public static DateTime AccessTokenExpiresOn(this IIdentity identity)
        {
            return GetExpDateTimeOffset(identity as ClaimsIdentity).DateTime;
        }

        public static DateTime AccessTokenExpiresOnUtc(this IIdentity identity)
        {
            return GetExpDateTimeOffset(identity as ClaimsIdentity).UtcDateTime;
        }

        private static DateTimeOffset GetExpDateTimeOffset(ClaimsIdentity identity)
        {
            if (null != identity)
            {
                var expTokenClaim = identity.Claims.FirstOrDefault(c => c.Type.Equals(tokenExpirationClaimType, StringComparison.InvariantCultureIgnoreCase));
                long expTokenTicks = 0;
                if (null != expTokenClaim)
                {
                    long.TryParse(expTokenClaim.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out expTokenTicks);

                    return DateTimeOffset.FromUnixTimeSeconds(expTokenTicks);
                }
            }

            return DateTimeOffset.FromUnixTimeSeconds(0);
        }
    }
}
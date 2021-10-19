using System;

namespace Arc4u.IdentityModel.Claims
{
    public class ClaimTypes
    {
        public static String Culture { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/culture"; } }

        public static String Authorization { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/authorization"; } }

        public static String ServiceSid { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/servicesid"; } }

        public static String Company { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/company"; } }

        public static String Sid { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/sid"; } }

        public static String UserPicture { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/userPicture"; } }

        public static String Name { get { return "name"; } }

        public static String Surname { get { return "family_name"; } }

        public static String GivenName { get { return "given_name"; } }

        public static String Upn { get { return "upn"; } }

        public static String Email { get { return "email"; } }

        public static String PrimarySid { get { return "primarysid"; } }

        public static String ObjectIdentifier { get { return "http://schemas.microsoft.com/identity/claims/objectidentifier"; } }

        public static String OID { get { return "oid"; } }
    }
}

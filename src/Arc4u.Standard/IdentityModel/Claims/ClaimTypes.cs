namespace Arc4u.IdentityModel.Claims;

public class ClaimTypes
{
    public static string Culture { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/culture"; } }

    public static string Authorization { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/authorization"; } }

    public static string ServiceSid { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/servicesid"; } }

    public static string Company { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/company"; } }

    public static string Sid { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/sid"; } }

    public static string UserPicture { get { return "http://schemas.arc4u.net/ws/2012/05/identity/claims/userPicture"; } }

    public static string Name { get { return "name"; } }

    public static string Surname { get { return "family_name"; } }

    public static string GivenName { get { return "given_name"; } }

    public static string Upn { get { return "upn"; } }

    public static string Email { get { return "email"; } }

    public static string PrimarySid { get { return "primarysid"; } }

    public static string ObjectIdentifier { get { return "http://schemas.microsoft.com/identity/claims/objectidentifier"; } }

    public static string OID { get { return "oid"; } }
}

using System.Text;
using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.Net;

[Export(typeof(IBasicAuthorizationHeader)), Shared]
public class BasicAuthorizationHeader : IBasicAuthorizationHeader
{
    public BasicAuthorizationHeader(ISecureCache secureCache)
    {
        _secureCache = secureCache;
    }

    private readonly ISecureCache _secureCache;

    public string GetHeader(IKeyValueSettings settings)
    {
        ExtractFromSettings(settings, out var passwordStoreKey);

        var userkey = passwordStoreKey + "_upn";
        var pwdkey = passwordStoreKey + "_pwd";

        _secureCache.TryGetValue<string>(userkey, out var upn);
        _secureCache.TryGetValue<string>(pwdkey, out var pwd);

        if (string.IsNullOrWhiteSpace(upn) || string.IsNullOrWhiteSpace(pwd))
        {
            throw new InvalidOperationException("The upn or password is not set in the cache!");
        }

        return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{upn!.Trim()}:{pwd!.Trim()}"))}";
    }

    private static void ExtractFromSettings(IKeyValueSettings settings, out string passwordStoreKey)
    {
        passwordStoreKey = "secret";
        if (settings.Values.ContainsKey(TokenKeys.PasswordStoreKey))
        {
            passwordStoreKey = string.IsNullOrWhiteSpace(settings.Values[TokenKeys.PasswordStoreKey]) ? passwordStoreKey : settings.Values[TokenKeys.PasswordStoreKey];
        }

    }
}

using System.Text;
using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;

namespace Arc4u.OAuth2.Net
{
    [Export(typeof(IBasicAuthorizationHeader)), Shared]
    public class BasicAuthorizationHeader : IBasicAuthorizationHeader
    {
        public BasicAuthorizationHeader(ISecureCache secureCache)
        {
            secureCache = secureCache;
        }

        private readonly ISecureCache secureCache;

        public string GetHeader(IKeyValueSettings settings)
        {
            ExtractFromSettings(settings, out var passwordStoreKey);

            var userkey = passwordStoreKey + "_upn";
            var pwdkey = passwordStoreKey + "_pwd";

            secureCache.TryGetValue<string>(userkey, out var upn);
            secureCache.TryGetValue<string>(pwdkey, out var pwd);

            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{upn.Trim()}:{pwd.Trim()}"))}";
        }

        private void ExtractFromSettings(IKeyValueSettings settings, out string passwordStoreKey)
        {
            passwordStoreKey = "secret";
            if (settings.Values.ContainsKey(TokenKeys.PasswordStoreKey))
            {
                passwordStoreKey = string.IsNullOrWhiteSpace(settings.Values[TokenKeys.PasswordStoreKey]) ? passwordStoreKey : settings.Values[TokenKeys.PasswordStoreKey];
            }

        }
    }
}

using Arc4u.Dependency.Attribute;
using Microsoft.Identity.Client;

namespace Arc4u.OAuth2.Msal.TokenProvider.Client
{
    [Export, Shared]
    public class PublicClientApp
    {
        public IPublicClientApplication? PublicClient { get; set; }
    }
}
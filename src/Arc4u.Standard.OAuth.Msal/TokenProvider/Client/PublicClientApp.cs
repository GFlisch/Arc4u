using Arc4u.Dependency.Attribute;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;

namespace Arc4u.OAuth2.Msal.TokenProvider.Client
{
    [Export, Shared]
    public class PublicClientApp
    {
        public IPublicClientApplication PublicClient { get; set; }

        public void SetCustomWebUi(Func<ICustomWebUi> customWebUi)
        {
            _customWebUi = customWebUi;
        }
        private Func<ICustomWebUi> _customWebUi;

        public ICustomWebUi CustomWebUi => HasCustomWebUi ? _customWebUi() : null;

        public bool HasCustomWebUi => _customWebUi is not null;
    }
}
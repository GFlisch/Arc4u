using Arc4u.Dependency.Attribute;
using System.Security.Claims;
using System.Threading;

namespace Arc4u.Security.Principal
{
    [Export(typeof(IApplicationContext)), Shared]

    public class ApplicationClaimsPrincipalSelectorContext : IApplicationContext
    {
        public AppPrincipal Principal => ClaimsPrincipal.Current as AppPrincipal;

        public void SetPrincipal(AppPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
        }
    }
}

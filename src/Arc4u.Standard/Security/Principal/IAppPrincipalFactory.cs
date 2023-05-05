using System.Threading;
using System.Threading.Tasks;
using Arc4u.ServiceModel;

namespace Arc4u.Security.Principal
{
    public interface IAppPrincipalFactory
    {
        Task<AppPrincipal> CreatePrincipal(Messages messages, object parameter = null);
        Task<AppPrincipal> CreatePrincipal(IKeyValueSettings settings, Messages messages, object parameter = null);
        Task<AppPrincipal> CreatePrincipal(string settingsResolveName, Messages messages, object parameter = null);

        ValueTask SignOutUserAsync(CancellationToken cancellationToken);

        ValueTask SignOutUserAsync(IKeyValueSettings settings, CancellationToken cancellationToken);
    }
}

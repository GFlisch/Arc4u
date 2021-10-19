using Arc4u.ServiceModel;
using System.Threading.Tasks;

namespace Arc4u.Security.Principal
{
    public interface IAppPrincipalFactory
    {
        Task<AppPrincipal> CreatePrincipal(Messages messages, object parameter = null);
        Task<AppPrincipal> CreatePrincipal(IKeyValueSettings settings, Messages messages, object parameter = null);
        Task<AppPrincipal> CreatePrincipal(string settingsResolveName, Messages messages, object parameter = null);

        void SignOutUser();

        void SignOutUser(IKeyValueSettings settings);
    }
}
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace Arc4u.Windows.Mvvm
{
    public interface ILaunchNavigation
    {
        Task OnStartApplicationAsync(IActivatedEventArgs launchArgs);
    }
}

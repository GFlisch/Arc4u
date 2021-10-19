using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arc4u.Xamarin.Forms.Mvvm
{
    public interface INavigationAware
    {
        Task OnNavigatingToAsync(IReadOnlyDictionary<string, object> parameters);

        Task OnNavigatingFromAsync();
    }
}

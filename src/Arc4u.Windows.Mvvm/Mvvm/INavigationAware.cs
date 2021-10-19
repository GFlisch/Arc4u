using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arc4u.Windows.Mvvm
{
    public interface INavigationAware
    {
        Task OnNavigatedFromAsync();

        Task OnNavigatedToAsync(IReadOnlyDictionary<string, object> parameters);

        Task<bool> OnNavigatingFromAsync(bool isBack);
    }
}

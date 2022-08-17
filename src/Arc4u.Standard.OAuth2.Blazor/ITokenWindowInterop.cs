using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Arc4u.Blazor
{
    public interface ITokenWindowInterop
    {
        public Task OpenWindowAsync(IJSRuntime jsRuntime, ILocalStorageService localStorage, string url, string feature = "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500");
    }
}

using Arc4u.Dependency.Attribute;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Arc4u.Blazor
{
    [Export(typeof(ITokenWindowInterop)), Shared]
    public class WindowInterop : ITokenWindowInterop
    {
        // Virtual and non static for Mocking purpose.
        public async Task OpenWindowAsync(IJSRuntime jsRuntime, ILocalStorageService localStorage, string url, string feature = "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500")
        {

            if (await localStorage.ContainKeyAsync("token"))
                await localStorage.RemoveItemAsync("token");

            if (await localStorage.ContainKeyAsync("fetching"))
                await localStorage.RemoveItemAsync("fetching");


            await jsRuntime.InvokeVoidAsync("Arc4u.CreatePopupWindow", url, feature);

            do
            {
                await Task.Delay(100);
            }
            while (null == await localStorage.GetItemAsync<string>("fetching"));

            await localStorage.RemoveItemAsync("fetching");
        }
    }
}

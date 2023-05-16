using Arc4u.Dependency.Attribute;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Arc4u.Blazor
{
    /// <summary>
    /// Provides a mechanism to interact with an OAuth token window.
    /// </summary>
    [Export(typeof(ITokenWindowInterop)), Shared]
    public class WindowInterop : ITokenWindowInterop
    {
        /// <summary>
        /// Opens a new window asynchronously with the specified URL and features, using the provided JS runtime and local storage service.
        /// Also, it handles the local storage entries for "token" and "fetching" before opening the window.
        /// The method waits for the "fetching" entry to be set in local storage, signaling the completion of token fetching, before returning.
        /// </summary>
        /// <param name="jsRuntime">The JS runtime to use for opening the window.</param>
        /// <param name="localStorage">The local storage service to use for storing and retrieving tokens.</param>
        /// <param name="url">The URL to open in the new window.</param>
        /// <param name="feature">The features of the new window. Defaults to "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500".</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OpenWindowAsync(IJSRuntime jsRuntime, ILocalStorageService localStorage, string url, string feature = "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500")
        {
            // Virtual and non static for Mocking purpose.

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

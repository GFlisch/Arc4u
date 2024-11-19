using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace Arc4u.Blazor
{
    /// <summary>
    /// Defines a contract for interacting with an OAuth token window.
    /// </summary>
    public interface ITokenWindowInterop
    {
        /// <summary>
        /// Opens a new window asynchronously with the specified URL and features, using the provided JS runtime and local storage service.
        /// </summary>
        /// <param name="jsRuntime">The JS runtime to use for opening the window.</param>
        /// <param name="localStorage">The local storage service to use for storing and retrieving tokens.</param>
        /// <param name="url">The URL to open in the new window.</param>
        /// <param name="feature">The features of the new window. Defaults to "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500".</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task OpenWindowAsync(IJSRuntime jsRuntime, ILocalStorageService localStorage, string url, string feature = "toolbar=no,scrollbars=yes,resizable=yes,top=500,left=500,width=450,height=500");
    }
}

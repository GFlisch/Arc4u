using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Blazor
{
    /// <summary>
    /// Provides extension methods for setting the default culture.
    /// </summary>
    public static class SetCultureUIExtensions
    {
        /// <summary>
        /// Sets the default culture for the application from local storage.
        /// </summary>
        /// <param name="host">The WebAssemblyHost instance on which to set the default culture.</param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <remarks>If no culture is found in local storage or an error occurs, this method will fail silently.</remarks>
        public static async Task SetDefaultCulture(this WebAssemblyHost host)
        {
            try
            {
                var localStorage = host.Services.GetService<Blazored.LocalStorage.ILocalStorageService>();

                var culture = await localStorage.GetItemAsync<string>("uiculture");

                if (!string.IsNullOrEmpty(culture))
                {
                    var cultureInfo = new CultureInfo(culture);
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                }

            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}

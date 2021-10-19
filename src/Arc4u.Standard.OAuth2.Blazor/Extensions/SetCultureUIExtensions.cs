using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Arc4u.Blazor
{
    public static class SetCultureUIExtensions
    {
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
            }
        }
    }
}

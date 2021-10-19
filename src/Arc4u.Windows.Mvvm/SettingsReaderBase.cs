using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace Arc4u.Windows
{
    /// <summary>
    /// Read a json file based on Key,Value and used to read settings.
    /// </summary>
    public abstract class SettingsReaderBase : IKeyValueSettings
    {
        private IReadOnlyDictionary<String, String> _values;

        public IReadOnlyDictionary<string, string> Values
        {
            get
            {
                return _values;
            }
        }

        public async Task InitializeAsync(string file)
        {
            var serializer = new DataContractJsonSerializer(typeof(Dictionary<String, String>));

            var sf = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{file}"));
            var text = await FileIO.ReadTextAsync(sf);

            var values = serializer.ReadObject<Dictionary<String, String>>(text);

            // Add the redirect uri => which is fixed with Uwp.
            if (!values.ContainsKey("RedirectUrl"))
            {
                string ApplicationRegisteringURI = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri;
                values.Add("RedirectUrl", ApplicationRegisteringURI);
            }

            _values = values;
        }

        public abstract Task InitializeAsync();


    }
}

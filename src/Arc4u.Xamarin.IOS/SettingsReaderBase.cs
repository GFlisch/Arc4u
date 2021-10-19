using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Arc4u
{
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

        public Task InitializeAsync(string file)
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Dictionary<String, String>));

            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            using (var stream = assembly.GetManifestResourceStream(file))
                _values = serializer.ReadObject(stream) as Dictionary<String, String>;

            return Task.FromResult<Object>(null);
        }

        public abstract Task InitializeAsync();

    }
}

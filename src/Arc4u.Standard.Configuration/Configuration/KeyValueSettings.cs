using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration
{
    public abstract class KeyValueSettings : IKeyValueSettings
    {
        private readonly Dictionary<string, string> _properties;

        public KeyValueSettings(string sectionName, IConfiguration configuration)
        {
            _properties = configuration.GetSection(sectionName)?.GetChildren()?.ToDictionary(x => x.Key, x => x.Value!) ?? throw new ArgumentException($"Section {sectionName} does not exist or doesn't contain a usable value", nameof(sectionName));
        }

        public IReadOnlyDictionary<string, string> Values => _properties;
    }
}

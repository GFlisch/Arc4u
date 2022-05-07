using System.Collections.Generic;

namespace Arc4u.Configuration
{
    public class SimpleKeyValueSettings : IKeyValueSettings
    {
        public SimpleKeyValueSettings(Dictionary<string, string> keyValues)
        {
            _keyValues = keyValues;
        }

        public static SimpleKeyValueSettings CreateFrom(IKeyValueSettings source)
        {
            var keyValues = new Dictionary<string, string>();

            foreach (var keyPair in source.Values)
                keyValues.Add(keyPair.Key, keyPair.Value);

            return new SimpleKeyValueSettings(keyValues);
        }

        public void Add(string key, string value)
        {
            _keyValues.Add(key, value);
        }

        private readonly Dictionary<string, string> _keyValues;

        public IReadOnlyDictionary<string, string> Values => _keyValues;

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var value in Values)
            {
                hash ^= value.GetHashCode();
            }

            return hash;
        }
    }
}

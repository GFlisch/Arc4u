using Arc4u.Caching.Memory;
using System.Collections.Generic;

namespace Arc4u.Standard.UnitTest.Infrastructure
{
    class MemorySettings : IKeyValueSettings
    {
        public MemorySettings()
        {
            _values = new Dictionary<string, string>();
            _values.Add(MemoryCache.CompactionPercentageKey, "0.2");
            _values.Add(MemoryCache.SizeLimitKey, "100000");
        }

        private Dictionary<string, string> _values;

        public IReadOnlyDictionary<string, string> Values => _values;

    }
}

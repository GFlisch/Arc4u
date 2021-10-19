using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Arc4u.Configuration
{
    public abstract class KeyValueSettings : IKeyValueSettings
    {
        public KeyValueSettings(string sectionName, IConfiguration configuration)
        {
            Properties = configuration.GetSection(sectionName).Get<Dictionary<String, String>>();
        }

        private Dictionary<String, String> Properties { get; }

        public IReadOnlyDictionary<string, string> Values => Properties;
    }
}

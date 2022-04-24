using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Arc4u
{
    [Export(typeof(IAppSettings)), Shared]
    public sealed class AppSettings : IAppSettings
    {
        public AppSettings(IConfiguration configuration)
        {
            Properties = configuration.GetSection("AppSettings").Get<Dictionary<String, String>>() ?? new Dictionary<string, string>();
        }

        private Dictionary<String, String> Properties { get; }

        public IReadOnlyDictionary<string, string> Values => Properties;
    }
}

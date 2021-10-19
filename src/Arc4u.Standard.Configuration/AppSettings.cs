using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Composition;

namespace Arc4u
{
    [Export(typeof(IAppSettings)), Shared]
    public sealed class AppSettings : IAppSettings
    {
        [ImportingConstructor]
        public AppSettings(IConfiguration configuration)
        {
            Properties = configuration.GetSection("AppSettings").Get<Dictionary<String, String>>() ?? new Dictionary<string, string>();
        }

        private Dictionary<String, String> Properties { get; }

        public IReadOnlyDictionary<string, string> Values => Properties;
    }
}

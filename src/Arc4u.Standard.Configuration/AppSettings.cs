using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.Configuration;

namespace Arc4u
{
    [Export(typeof(IAppSettings)), Shared]
    public sealed class AppSettings : KeyValueSettings, IAppSettings
    {
        public AppSettings(IConfiguration configuration)
            : base("AppSettings", configuration)
        {
        }
    }
}

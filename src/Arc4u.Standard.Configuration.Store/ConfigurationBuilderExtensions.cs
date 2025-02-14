using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration.Store;

using Internals;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Define which sections need to be persisted
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddSectionStoreConfiguration(this IConfigurationBuilder builder, Action<ISectionStoreConfigurationOptions> configure)
    {
        var options = new SectionStoreConfigurationOptions();
        configure(options);
        return builder.Add(new SectionStoreConfigurationSource(options));
    }
}

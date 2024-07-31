namespace Arc4u.Configuration.Store;

/// <summary>
/// Configuration options for persisted configuration sections
/// </summary>
public interface ISectionStoreConfigurationOptions
{
    /// <summary>
    /// Add a section with a specific value to be persisted
    /// </summary>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <param name="key">the name of the section</param>
    /// <param name="value">the value of the section</param>
    /// <returns></returns>
    ISectionStoreConfigurationOptions Add<TValue>(string key, TValue? value);


    /// <summary>
    /// Add a section from the existing configuration to be persisted.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the existing configuration</typeparam>
    /// <param name="key">the name of the section</param>
    /// <returns></returns>
    ISectionStoreConfigurationOptions Add<TValue>(string key);
}

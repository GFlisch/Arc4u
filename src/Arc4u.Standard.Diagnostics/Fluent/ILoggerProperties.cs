namespace Arc4u.Diagnostics;

/// <summary>
/// This allows us to write methods adding properties to any type deriving from <see cref="BaseLoggerProperties"/> in a fluent way.
/// This is currently both <see cref="CommonLoggerProperties"/> and <see cref="MonitoringLoggerProperties"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILoggerProperties<T>
    where T : BaseLoggerProperties
{
    T Add(string key, int value);
    T Add(string key, double value);
    T Add(string key, bool value);
    T Add(string key, long value);
    T Add(string key, string value);
    void Log();
}


namespace Arc4u.Diagnostics;
public interface IArc4uLogger<T>
{
    ILoggerWrapper<T> SetContext(string category, string caller = "", Type? realType = null);
}

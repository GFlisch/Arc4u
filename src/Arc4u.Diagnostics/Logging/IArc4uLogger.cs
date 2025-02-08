
namespace Arc4u.Diagnostics;
public interface IArc4uLogger<T>
{
    LoggerWrapper<T> SetContext(string category, string caller = "", Type? realType = null);
}

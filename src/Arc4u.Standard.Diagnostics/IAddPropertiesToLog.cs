namespace Arc4u.Diagnostics;

public interface IAddPropertiesToLog
{
    IDictionary<string, object> GetProperties();
}

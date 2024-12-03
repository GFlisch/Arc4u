namespace Arc4u.Diagnostics.Serilog.Sinks;

public interface ILogStore
{
    List<LogMessage> GetLogs(string criteria, int skip, int take);

    void RemoveAll();
}

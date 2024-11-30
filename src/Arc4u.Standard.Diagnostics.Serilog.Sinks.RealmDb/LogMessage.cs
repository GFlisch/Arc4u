namespace Arc4u.Diagnostics.Serilog.Sinks;

public class LogMessage
{
    public DateTimeOffset Timestamp { get; set; }

    public string MessageCategory { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string MessageType { get; set; } = string.Empty;

    public string ClassType { get; set; } = string.Empty;

    public string MethodName { get; set; } = string.Empty;

    public int ThreadId { get; set; }

    public int ProcessId { get; set; }

    public string Application { get; set; } = string.Empty;

    public string Identity { get; set; } = string.Empty;

    public string ActivityId { get; set; } = string.Empty;

    public string Stacktrace { get; set; } = string.Empty;

    public string Properties { get; set; } = string.Empty;

}

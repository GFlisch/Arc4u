using Realms;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb;

public class LogDBMessage : RealmObject
{
    public DateTimeOffset Timestamp { get; set; }

    public short MessageCategory { get; set; }

    public string Message { get; set; } = default!;

    public int MessageType { get; set; }

    public string ClassType { get; set; } = default!;

    public string MethodName { get; set; } = default!;

    public int ThreadId { get; set; }

    public int ProcessId { get; set; }

    public string Application { get; set; } = default!;

    public string Identity { get; set; } = default!;

    public string ActivityId { get; set; } = default!;

    public string Stacktrace { get; set; } = default!;

    public string Properties { get; set; } = default!;

}

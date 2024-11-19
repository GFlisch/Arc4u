namespace Arc4u.Diagnostics.Serilog.Sinks
{
    public class LogMessage
    {
        public DateTimeOffset Timestamp { get; set; }

        public string MessageCategory { get; set; }

        public string Message { get; set; }

        public string MessageType { get; set; }

        public string ClassType { get; set; }

        public string MethodName { get; set; }

        public int ThreadId { get; set; }

        public int ProcessId { get; set; }

        public string Application { get; set; }

        public string Identity { get; set; }

        public string ActivityId { get; set; }

        public string Stacktrace { get; set; }

        public string Properties { get; set; }

    }
}

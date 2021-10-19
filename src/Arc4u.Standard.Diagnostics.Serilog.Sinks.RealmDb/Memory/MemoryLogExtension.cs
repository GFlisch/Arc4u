using Serilog;
using Serilog.Configuration;

namespace Arc4u.Diagnostics.Serilog.Sinks.Memory
{
    public static class MemoryLogExtension
    {
        public static LoggerConfiguration MemoryLogDB(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new MemoryLogDbSink());
        }

    }
}

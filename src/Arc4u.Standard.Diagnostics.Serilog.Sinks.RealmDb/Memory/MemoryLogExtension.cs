using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace Arc4u.Diagnostics.Serilog.Sinks.Memory;

public static class MemoryLogExtension
{
    public static LoggerConfiguration MemoryLogDB(this LoggerSinkConfiguration loggerConfiguration)
    {
        var sink = new MemoryLogDbSink();

        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = 50,
            Period = TimeSpan.FromMilliseconds(500),
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000
        };

        var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);

        return loggerConfiguration.Sink(batchingSink);
    }

}

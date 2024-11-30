using Arc4u.Diagnostics.Serilog.Sinks.RealmDb;
using Realms;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog;

public static class RealmDBExtension
{
    public static LoggerConfiguration RealmDB(this LoggerSinkConfiguration loggerConfiguration)
    {
        var sink = new RealmDBSink(DefaultConfig());

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

    public static Func<RealmConfiguration> DefaultConfig = () => new RealmConfiguration("loggingDB.realm")
    {
        SchemaVersion = 1,
    };

}


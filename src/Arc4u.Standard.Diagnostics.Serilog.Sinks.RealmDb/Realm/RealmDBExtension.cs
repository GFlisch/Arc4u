using Arc4u.Diagnostics.Serilog.Sinks.RealmDb;
using Realms;
using Serilog.Configuration;
using System;

namespace Serilog
{
    public static class RealmDBExtension
    {
        public static LoggerConfiguration RealmDB(this LoggerSinkConfiguration loggerConfiguration)
        {
            return loggerConfiguration.Sink(new RealmDBSink(DefaultConfig()));
        }

        public static Func<RealmConfiguration> DefaultConfig = () => new RealmConfiguration("loggingDB.realm")
        {
            SchemaVersion = 1,
        };

    }
}


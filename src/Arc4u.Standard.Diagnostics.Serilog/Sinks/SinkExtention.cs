using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Sinks;
using Serilog.Configuration;
using Serilog.Core;

namespace Serilog
{
    public static class SinkExtention
    {
        /// <summary>
        /// Add a category filter.
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="category"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public static LoggerConfiguration CategoryFilter(this LoggerSinkConfiguration loggerConfiguration, MessageCategory category, ILogEventSink sink)
        {
            return loggerConfiguration.Sink(new CategoryFilterSink(category, sink));
        }

        /// <summary>
        /// Suppress IdentityName
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="sink"></param>
        /// <returns></returns>
        public static LoggerConfiguration Anonymizer(this LoggerSinkConfiguration loggerConfiguration, ILogEventSink sink)
        {
            return loggerConfiguration.Sink(new AnonymizerSink(sink));
        }
    }
}

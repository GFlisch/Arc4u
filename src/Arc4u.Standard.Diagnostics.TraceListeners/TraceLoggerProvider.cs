using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Arc4u.Diagnostics
{
    public sealed class TraceLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, TraceLogger> _loggers = new ConcurrentDictionary<string, TraceLogger>();
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new TraceLogger(name));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}

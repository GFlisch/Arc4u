using NServiceBus.Logging;
using System;

namespace Arc4u.NServiceBus.Diagnostics
{
    public class LoggerFactory : ILoggerFactory
    {
        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new LoggerBridge();
        }
    }
}

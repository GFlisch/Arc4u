using Arc4u.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.Diagnostics
{

    /// <summary>
    /// The class  is used to log any information on different media.
    /// </summary>
    public class Logger : LoggerBase
    {
        private static object _locker = new object();

        private static string Unknow = "Unknow";

        private static bool _isInitialised = false;

        public static void Initialize(Config config, ILogger logger = null)
        {
            if (null == config)
                throw new ArgumentNullException(nameof(config));

            lock (_locker)
            {
                try
                {
                    if (_isInitialised) return;

                    if (null != logger)
                        LoggerInstance = logger;

                    DoInitialize(config);

                    _isInitialised = true;
                }
                catch (Exception)
                {
                }
            }
        }

        // Scope a ILogger => for unit testing purpose.
        // In .Net 5 only this will be replaced by a Scoped version of the ILogger in the DI.
        [Obsolete("Scope a ILogger => for unit testing purpose. With DI Core, used the dependency injection (Scoped).")]
        public static Scope<ILogger> InitializeInScope(ILogger logger)
        {
            if (null == logger)
                throw new ArgumentNullException(nameof(logger));

            var scope = new Scope<ILogger>(logger);

            return scope;
        }

        private static void DoInitialize(Config config)
        {
            Logger.Application = config.Environment.LoggingName.Trim().Length > 0 ? config.Environment.LoggingName.Trim() : Unknow;
            Logger.FilterLevel = config.Environment.LoggingLevel;
        }


    }
}

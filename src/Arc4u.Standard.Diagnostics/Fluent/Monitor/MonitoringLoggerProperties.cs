namespace Arc4u.Diagnostics
{
    public class MonitoringLoggerProperties : BaseLoggerProperties, ILoggerProperties<MonitoringLoggerProperties>
    {
        internal MonitoringLoggerProperties(LoggerMessage loggerMessage) : base(loggerMessage) { }

        public MonitoringLoggerProperties AddMemoryUsage()
        {
            return Add("Memory", GC.GetTotalMemory(false));

        }

        public MonitoringLoggerProperties Add(string key, string value)
        {
            AddProperty(key, value);
            return this;
        }

        public MonitoringLoggerProperties Add(string key, int value)
        {
            AddProperty(key, value);
            return this;
        }

        public MonitoringLoggerProperties Add(string key, double value)
        {
            AddProperty(key, value);
            return this;
        }

        public MonitoringLoggerProperties Add(string key, bool value)
        {
            AddProperty(key, value);
            return this;
        }

        public MonitoringLoggerProperties Add(string key, long value)
        {
            AddProperty(key, value);
            return this;
        }

    }
}

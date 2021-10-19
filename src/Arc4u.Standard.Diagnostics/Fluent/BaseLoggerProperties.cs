namespace Arc4u.Diagnostics
{
    public abstract class BaseLoggerProperties
    {
        protected private readonly LoggerMessage LoggerMessage;

        internal BaseLoggerProperties(LoggerMessage loggerMessage)
        {
            LoggerMessage = loggerMessage;
        }

        internal void AddProperty(string key, object value)
        {
            // If the property already exists, it will be updated. Otherwise, it will be added
            LoggerMessage.Properties[key] = value;
        }

        /// <summary>
        /// Log the message.
        /// </summary>
        public void Log()
        {
            LoggerMessage.Log();
        }
    }
}

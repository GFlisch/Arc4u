using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc4u.Diagnostics
{
    public class LoggerContext : IDisposable
    {
        /// <summary>
        /// Create a <see cref="LoggerContext"/> and copy the properties or not regarding
        /// the <see cref="PropertyFilter"></see> value.
        /// </summary>
        /// <param name="filter">All or None.</param>
        public LoggerContext(PropertyFilter filter = PropertyFilter.All)
        {
            if (filter == PropertyFilter.All)
            {
                if (null == Current?.Properties)
                    Properties = new List<KeyValuePair<string, object>>();
                else
                    Properties = new List<KeyValuePair<string, object>>(Current?.Properties);
            }
            else
                Properties = new List<KeyValuePair<string, object>>();

            toDispose = new Scope<LoggerContext>(this);

        }

        /// <summary>
        /// Create a new <see cref="LoggerContext"/> and copy the existing properties based
        /// on the keepItOrNot function.
        /// </summary>
        /// <param name="keepItOrNot">Function to select if we keep or not the property.</param>
        public LoggerContext(Func<KeyValuePair<string, object>, bool> keepItOrNot)
        {
            Properties = new List<KeyValuePair<string, object>>();

            if (null != Current?.Properties)
            {
                foreach (var property in Current.Properties)
                    if (keepItOrNot(property)) Properties.Add(property);
            }

            toDispose = new Scope<LoggerContext>(this);

        }

        private IDisposable toDispose;

        public static LoggerContext Current { get { return Scope<LoggerContext>.Current; } }

        private List<KeyValuePair<string, object>> Properties { get; set; }

        public IReadOnlyList<KeyValuePair<string, object>> All()
        {
            return new List<KeyValuePair<string, object>>(Current?.Properties);
        }

        internal void AddValue(string key, object value)
        {

            switch (key)
            {
                case LoggingConstants.ActivityId:
                case LoggingConstants.Application:
                case LoggingConstants.Category:
                case LoggingConstants.Class:
                case LoggingConstants.Identity:
                case LoggingConstants.MethodName:
                case LoggingConstants.ProcessId:
                case LoggingConstants.Stacktrace:
                case LoggingConstants.ThreadId:
                case LoggingConstants.Type:
                    throw new ReservedLoggingKeyException(key);
            }

            var existingValue = Current.Properties.FirstOrDefault(kv => kv.Key.Equals(key));
            if (null == existingValue.Key)
                Properties.Add(new KeyValuePair<string, object>(key, value));
            else
            {
                Properties.Remove(existingValue);
                Properties.Add(new KeyValuePair<string, object>(key, value));
            }
        }

        public void Add(string key, int value)
        {
            Current.AddValue(key, value);
        }

        public void Add(string key, double value)
        {
            Current.AddValue(key, value);
        }

        public void Add(string key, bool value)
        {
            Current.AddValue(key, value);
        }

        public void Add(string key, long value)
        {
            Current.AddValue(key, value);
        }

        public void Dispose()
        {
            toDispose?.Dispose();
        }
    }
}

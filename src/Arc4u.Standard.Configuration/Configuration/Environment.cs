using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization;

namespace Arc4u.Configuration
{
    [DataContract]
    public class Environment
    {
        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "loggingName")]
        public String LoggingName { get; set; }

        [DataMember(Name = "timeZone")]
        public String TimeZone { get; set; }

        [DataMember(Name = "loggingLevel")]
        public LogLevel LoggingLevel { get; set; } = LogLevel.Information;
    }
}

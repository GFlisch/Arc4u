using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Arc4u.Diagnostics
{
    [Obsolete("Use Serilog")]
    public class ConsoleTraceListener : TraceListener
    {
        protected override void Initialize()
        {
            Buffer = new MessageFileLoggerListenerStateInfo
            {
                Buffer = new List<TraceMessage>(),
                Locker = new object(),
                ResetEvent = new AutoResetEvent(false),
                RollingFile = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds).ToString(CultureInfo.InvariantCulture),
                LastFileRetentionDate = DateTime.Today.AddDays(-1)
            };
        }

        protected override void ProcessMessages(List<TraceMessage> messages)
        {
            foreach (var message in messages)
            {
                var description = new StringBuilder(message.FullText);
                if (null != message.Source && !String.IsNullOrWhiteSpace(message.Source.Stacktrace) && (message.Type == LogLevel.Error || message.Type == LogLevel.Critical))
                {
                    description.AppendLine();
                    description.Append(message.Source.Stacktrace);
                }

                string msg = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} - {9}",
                                                        Enum.GetName(typeof(LogLevel), message.Type).PadRight(13),
                                                        message.Category.ToString().PadRight(14),
                                                        message.Source.IdentityName.PadRight(15),
                                                        message.Source.ProcessId.ToString().PadRight(6),
                                                        message.Source.ThreadId.ToString().PadRight(6),
                                                        message.Source.EventId.ToString().PadRight(6),
                                                        String.IsNullOrWhiteSpace(message.ActivityId) ? string.Empty.PadRight(40) : message.ActivityId.PadRight(40),
                                                        message.Source.TypeName,
                                                        message.Source.MethodName,
                                                        description);
                Console.WriteLine(msg);
            }
        }
    }
}

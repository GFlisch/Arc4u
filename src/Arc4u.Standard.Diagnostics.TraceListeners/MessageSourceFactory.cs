using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

#if WINDOWS_UAP
using Windows.Networking.Connectivity;
using Windows.System.Diagnostics;
using System.Linq;
#endif


namespace Arc4u.Diagnostics
{
    public class MessageSourceFactory : IMessageSourceFactory
    {
        static int _processId;
        static String _machineName;
        static MessageSourceFactory()
        {
#if WINDOWS_UAP
            _processId = ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId;
            var localHost = NetworkInformation.GetHostNames().FirstOrDefault(host => host.DisplayName.Contains(".local"));
            _machineName = localHost?.DisplayName.Replace(".local", "");
#else
            _processId = Process.GetCurrentProcess().Id;
            _machineName = Process.GetCurrentProcess().MachineName;
#endif
        }

        public MessageSource Create(string application, int eventId)
        {
            return Create(Environment.StackTrace, application, eventId);
        }

        public MessageSource Create(string stackTrace, string application, int eventId)
        {
            var messageSource = new MessageSource();

            if (!string.IsNullOrEmpty(stackTrace))
            {
                messageSource.Stacktrace = ExtractInfoFromStackTrace(stackTrace, out string methodName, out string typeName);
                messageSource.TypeName = typeName;
                messageSource.MethodName = methodName;
            }

            messageSource.Date = DateTime.UtcNow;
            messageSource.MachineName = _machineName;
            messageSource.ProcessId = _processId;
            messageSource.IdentityName = Thread.CurrentPrincipal.Identity.Name ?? String.Empty;
            messageSource.ThreadId = Environment.CurrentManagedThreadId;
            messageSource.EventId = eventId;
            messageSource.Application = application;

            return messageSource;
        }

        private static String ExtractInfoFromStackTrace(string stackTrace, out string method, out string typeName)
        {
            method = string.Empty;
            typeName = string.Empty;

            // Extract Method Name and Type from the stacktrace.
            var traces = new StringReader(stackTrace);
            var output = new StringWriter();
            bool write = false;
            String trace;
            while (null != (trace = traces.ReadLine()))
            {
                if (!write)
                {
                    if (trace.Contains("System.Environment")) continue;
                    if (trace.Contains("Arc4u.Diagnostics")) continue;
                    if (trace.Contains("System.Diagnostics")) continue;
                    if (trace.Contains("Arc4u.ServiceModel.Aspect")) continue;
                    ExtractInfo(trace, out method, out typeName);
                }

                write = true;
                output.WriteLine(trace);
            }

            return output.ToString();

        }

        /// <summary>
        /// Analyse a line from the stack trace and extract the method name and type name.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="typeName"></param>
        private static void ExtractInfo(string trace, out string method, out string typeName)
        {
            method = string.Empty;
            typeName = string.Empty;

            if (String.IsNullOrEmpty(trace))
                return;

            var openParenthesePos = trace.IndexOf('(');
            if (-1 == openParenthesePos)
                return;

            var startTypeName = trace.LastIndexOf(' ', openParenthesePos) + 1;
            if (0 == startTypeName)
                return;

            typeName = trace.Substring(startTypeName, openParenthesePos - startTypeName);

            var startMethodName = typeName.LastIndexOf('.') + 1;
            if (startMethodName > 0)
            {
                method = typeName.Substring(startMethodName);
            }
        }


    }
}

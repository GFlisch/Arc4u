using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
namespace Arc4u.Diagnostics
{

    public class MessageFileLoggerListenerStateInfo : MessageBufferListenerStateInfo
    {
        public String RollingFile { get; set; }

        public DateTime LastFileRetentionDate { get; set; }
    }

    public class FileLoggerTraceListener : TraceListener
    {

        private const string MaxFileSizeKey = "maxFileSize";
        private const long max = 1073741824; // 1 GBytes

        /// <summary>
        /// Represents the key containing the value of the <see cref="MaxTraceFileDays"/> in the <see cref="TraceListener.AttributesCopy"/>.
        /// </summary>
        private const string MaxFileDaysKey = "maxRetentionFileDays";

        /// <summary>
        /// Represents the <see cref="String"/> directory where the log files will be stored.
        /// </summary>
        private const string DirectoryPathKey = "logDir";

        /// <summary>
        /// part of the name of the file generated!
        /// </summary>
        private const string ApplicationName = "applicationName";


        public FileLoggerTraceListener()
        {
        }

        public FileLoggerTraceListener(string name) : base(name)
        {
        }

        public FileLoggerTraceListener(StringDictionary attributes) : base(attributes)
        {
        }

        protected override void Initialize()
        {
            _retentionDays = GetRetentionDays();
            _maxFileSize = GetMaxFileSize();
            _directory = GetLogDirectory();
            _applicationName = GetApplicationName();

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
            var fileName = GetFileName((MessageFileLoggerListenerStateInfo)Buffer);

            if (null == fileName) // the path given in the config file does not exist!
                return;

            Stream fileWriter = null;

            try
            {
                if (messages.Count > 0)
                {
                    fileWriter = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                    WriteMessages(fileWriter, messages);
                }

            }
            catch (Exception loggerEx)
            {
                LogMessage($"{loggerEx.ToString()}");
            }
            finally
            {
                if (null != fileWriter) fileWriter.Dispose();
            }

        }

        private int _retentionDays;
        private long _maxFileSize;
        private string _directory;
        private string _applicationName;

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "maxFileSize", "maxRetentionFileDays", "logDir", "filters" };
        }


        /// <summary>
        /// Writes the message to the file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="copy">The messages to log.</param>
        protected void WriteMessages(Stream stream, List<TraceMessage> copy)
        {
            byte[] allMessages;
            using (var memoryStream = new MemoryStream(10000))
            {
                copy.ForEach(message =>
                {
                    try
                    {
                        WriteMessage(stream, message);
                    }
                    catch (Exception e)
                    {
                        LogMessage(e.ToString());
                    }
                });

                allMessages = new byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(allMessages, 0, allMessages.Length);
            }

            stream.Write(allMessages, 0, allMessages.Length);
        }

        protected void WriteMessage(Stream stream, TraceMessage message)
        {
            if (stream.Length == 0)
            {
                string msgHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                              "Date".PadRight(24),
                              "MessageType".PadRight(13),
                              "MessageKind".PadRight(14),
                              "User".PadRight(15),
                              "PID".PadRight(6),
                              "TID".PadRight(6),
                              "ID".PadRight(6),
                              "ActivityID".PadRight(40),
                              "Description");
                stream.Write(Encoding.Default.GetBytes(msgHeader),
                                   0, msgHeader.Length);

                // Add Carriage Return, Line feed.
                stream.Write(new byte[] { 0x0D, 0x0A }, 0, 2);

            }

            var description = new StringBuilder(message.FullText);
            if (null != message.Source && !String.IsNullOrWhiteSpace(message.Source.Stacktrace) && (message.Type.Equals("Error") || message.Type.Equals("Fatal")))
            {
                description.AppendLine();
                description.Append(message.Source.Stacktrace);
            }

            string msg = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} - {9} - {10}",
                                                                    message.Source.Date.ToString("dd/MM/yyyy HH:mm:ss,fff").PadRight(24),
                                                                    message.Type.ToString().PadRight(13),
                                                                    message.Category.ToString().PadRight(14),
                                                                    message.Source.IdentityName.PadRight(15),
                                                                    message.Source.ProcessId.ToString().PadRight(6),
                                                                    message.Source.ThreadId.ToString().PadRight(6),
                                                                    message.Source.EventId.ToString().PadRight(6),
                                                                    String.IsNullOrWhiteSpace(message.ActivityId) ? string.Empty.PadRight(40) : message.ActivityId.PadRight(40),
                                                                    message.Source.TypeName,
                                                                    message.Source.MethodName,
                                                                    description);

            stream.Write(Encoding.Default.GetBytes(msg), 0, msg.Length);
            // Add Carriage Return, Line feed.
            stream.Write(new byte[] { 0x0D, 0x0A }, 0, 2);
        }

        protected string GetFileName(MessageFileLoggerListenerStateInfo bufferStateInfo)
        {
            var rollingFile = bufferStateInfo.RollingFile;

            // Check if I need to roll the file because we reached the limit.
            var fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_{4}.log", _applicationName,
                                     DateTime.Today.Year.ToString("0000"),
                                     DateTime.Today.Month.ToString("00"),
                                     DateTime.Today.Day.ToString("00"),
                                     rollingFile);
            if (File.Exists(System.IO.Path.Combine(_directory, fileName)))
            {
                var fi = new FileInfo(System.IO.Path.Combine(_directory, fileName));
                if (fi.Length > _maxFileSize)
                {
                    rollingFile = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds).ToString();
                    bufferStateInfo.RollingFile = rollingFile;

                    fileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_{4}.log",
                                             _applicationName,
                                             DateTime.Today.Year.ToString("0000"),
                                             DateTime.Today.Month.ToString("00"),
                                             DateTime.Today.Day.ToString("00"),
                                             rollingFile);
                }
            }

            if (bufferStateInfo.LastFileRetentionDate < DateTime.Today)
            {
                try
                {
                    CleanLogFiles();

                    bufferStateInfo.LastFileRetentionDate = DateTime.Today;

                }
                catch (Exception ex)
                {
                    LogMessage(ex.ToString());
                }
            }

            return System.IO.Path.Combine(_directory, fileName);

        }

        protected void CleanLogFiles()
        {
            var limitDate = DateTime.Today.AddDays(-_retentionDays);
            IEnumerable<String> files = Directory.GetFiles(_directory, "*.log");

            foreach (var file in files)
            {
                if (File.GetLastWriteTime(file).Date <= limitDate)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Get the number of days we will keep the files in the directory.
        /// </summary>
        /// <returns>The number from the config. Default is 10.</returns>
        private int GetRetentionDays()
        {
            var result = 10; // default is 10 days.
            if (AttributesCopy.ContainsKey(MaxFileDaysKey))
            {
                int.TryParse(AttributesCopy[MaxFileDaysKey], out result);
            }

            return result;
        }

        private string GetApplicationName()
        {
            if (AttributesCopy.ContainsKey(ApplicationName))
            {
                return AttributesCopy[ApplicationName];
            }

            return String.Empty;
        }

        private long GetMaxFileSize()
        {

            long result = 10 * 1024 * 2014; // default is 10 MBytes.
            if (AttributesCopy.ContainsKey(MaxFileSizeKey))
            {
                if (long.TryParse(AttributesCopy[MaxFileSizeKey], out result))
                {
                    if (result > max) result = max;
                    else
                    {
                        result *= 1024 * 1024; // expressed in MB!
                        if (result > max) result = max;
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// return the directory given in the config fileand if nothing is mentionned, the CommonApplicationData folder is used.
        /// </summary>
        /// <returns>The <see cref="String"/> directory.</returns>
        private String GetLogDirectory()
        {
            var pathDoesntExist = false;

            if (AttributesCopy.ContainsKey(DirectoryPathKey))
            {
                if (IO.Path.IsRelative(AttributesCopy[DirectoryPathKey]))
                {
                    try
                    {
                        return IO.Path.MakeFullPath(AppDomain.CurrentDomain.BaseDirectory, AttributesCopy[DirectoryPathKey]);
                    }
                    catch
                    {
                        pathDoesntExist = true;
                    }
                }
                else
                {
                    try
                    {
                        if (Directory.Exists(AttributesCopy[DirectoryPathKey]))
                            return AttributesCopy[DirectoryPathKey];
                    }
                    catch (InvalidOperationException)
                    {
                        pathDoesntExist = true;
                    }
                }
            }

            if (pathDoesntExist)
            {
                LogMessage($"The path for the log file does not exist: {AttributesCopy[DirectoryPathKey]}.");
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        }
    }
}

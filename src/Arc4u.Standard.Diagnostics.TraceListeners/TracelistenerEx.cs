using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;

namespace Arc4u.Diagnostics
{

    public abstract class TracelistenerEx : TraceListener
    {
        /// <summary>
        /// Represents the key containing the value of the <see cref="MaxTraceFileDays"/> in the <see cref="TraceListener.AttributesCopy"/>.
        /// </summary>
        private const string MaxFileDaysKey = "maxRetentionFileDays";

        /// <summary>
        /// Represents the <see cref="String"/> directory where the log files will be stored.
        /// </summary>
        private const string DirectoryPathKey = "logDir";

        /// <summary>
        /// Determine the number of seconds between two check.
        /// </summary>
        private const string FrequencyKey = "frequency";

        private int _retentionDays;
        private string _directory;
        private TimeSpan _frequency;

        protected TracelistenerEx()
        {

        }

        protected TracelistenerEx(string name) : base(name)
        {

        }
        protected TracelistenerEx(StringDictionary attributes) : base(attributes)
        {

        }

        protected override void Initialize()
        {
            InitializeEx();

            _retentionDays = GetRetentionDays();
            _directory = GetLogDirectory();
            _frequency = GetFrequency();

            ThreadPool.QueueUserWorkItem(ProcessLostMessages, null);

        }

        /// <summary>
        /// When we have an issue to process a message, we save it in the Directory folder and try to resent it after!
        /// </summary>
        /// <param name="state"></param>
        void ProcessLostMessages(object state)
        {
            do
            {
                var deserializer = new DataContractSerializer(typeof(TraceMessage));
                try
                {
                    var failedCounter = 0;
                    IEnumerable<String> files = Directory.GetFiles(_directory, "*.msglog");

                    foreach (var file in files)
                    {
                        try
                        {
                            try
                            {
                                // Check if we have an empty file (issue during save).
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Length == 0)
                                {
                                    Delete(file);
                                    continue;
                                }
                            }
                            catch
                            {
                                // will retry for the next time. May be server resources will be better.
                                continue;
                            }

                            object msg = null;
                            try
                            {
                                deserializer.ReadObject(file, out msg);
                            }
                            catch
                            {
                                // message is lost.
                                Delete(file);
                                continue;
                            }


                            ProcessMessage(msg as TraceMessage);
                            Delete(file);
                        }
                        catch (Exception)
                        {
                            // Stop if we have too many erros. Do not parse all the messages for nothing. Probably the error is still there.
                            failedCounter++;
                            if (failedCounter > 20)
                                break;
                        }
                    }
                    CleanLogFiles();

                    Thread.Sleep(_frequency);
                }
                finally
                {

                }

            } while (!StopProcessing);
        }

        protected abstract void InitializeEx();

        protected abstract void ProcessMessage(TraceMessage message);

        protected override void ProcessMessages(List<TraceMessage> messages)
        {
            messages.ForEach(message =>
            {
                try
                {
                    ProcessMessage(message);
                }
                catch
                {
                    try
                    {
                        // Will save only on error!
                        Save(message);
                    }
                    catch
                    {
                        // message is lost!
                    }

                }

            });
        }

        // serialize a message in a folder so it can be processed after!
        protected void Save(TraceMessage msg)
        {
            if (String.IsNullOrWhiteSpace(_directory))
                return; // No backup functionality.

            var file = Path.Combine(_directory, Guid.NewGuid().ToString()) + ".msglog";
            try
            {
                var serializer = new DataContractSerializer(typeof(TraceMessage));

                serializer.WriteObject(file, msg);
            }
            catch (Exception)
            {
                // Check if a file is created (happens with a file size equal to zero or incomplete serialization or...
                // Message is lost.
                Delete(file);
            }


        }

        protected void Delete(String file)
        {
            try
            {
                File.Delete(file);
            }
            catch { }
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
                int.TryParse(AttributesCopy[MaxFileDaysKey], NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            }

            return result;
        }

        private TimeSpan GetFrequency()
        {
            TimeSpan result = TimeSpan.FromSeconds(10); // default is 10 seconds.
            if (AttributesCopy.ContainsKey(FrequencyKey))
            {
                TimeSpan.TryParse(AttributesCopy[FrequencyKey], CultureInfo.InvariantCulture, out result);
                if (TimeSpan.Zero == result)
                    result = TimeSpan.FromSeconds(10);
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
            // null path => no backup of messages.
            return null;
        }

        protected void CleanLogFiles()
        {
            var limitDate = DateTime.Today.AddDays(-_retentionDays);
            IEnumerable<String> files = Directory.GetFiles(_directory, "*.msglog");

            foreach (var file in files)
            {
                if (File.GetLastWriteTime(file).Date <= limitDate)
                {
                    Delete(file);
                }
            }
        }
    }
}

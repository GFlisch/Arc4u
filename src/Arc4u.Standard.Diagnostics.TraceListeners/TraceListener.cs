using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

namespace Arc4u.Diagnostics
{

    public class MessageBufferListenerStateInfo
    {
        public object Locker { get; set; }

        public List<TraceMessage> Buffer { get; set; }

        public AutoResetEvent ResetEvent { get; set; }

        public Timer Timer { get; set; }

    }


    public abstract class TraceListener : System.Diagnostics.TraceListener
    {
        protected TraceListener()
        {

        }

        protected TraceListener(string name) : base(name)
        {

        }
        protected TraceListener(StringDictionary attributes)
        {
            Setup(attributes);
        }

        protected const String EventSource = "Arc4u";
        protected const String EventSourceLog = "Application";
        protected bool StopProcessing;
        protected bool IsInitialized;

        protected MessageBufferListenerStateInfo Buffer;

        protected StringDictionary AttributesCopy;

        /// <summary>
        /// The attribute name used to reolve the filters to implement in the listener!
        /// </summary>
        private const string FiltersKey = "filters";

        /// <summary>
        /// Check if we have a <see cref="TraceMessage"/> as object and if only if ye we write to the file!
        /// </summary>
        /// <param name="o"></param>
        public override void Write(object o)
        {
            if (null == Buffer) return;

            if (!(o is TraceMessage)) return; // do nothing!

            var message = o as TraceMessage;

            Buffer.Buffer.Add(message);

        }

        public override void WriteLine(object o)
        {
            if (null == Buffer) return;

            if (!(o is TraceMessage)) return; // do nothing!

            var message = o as TraceMessage;

            Buffer.Buffer.Add(message);
        }
        /// <summary>
        /// do nothing by design. This listener is intended to bu used only with the object method!
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {

        }

        /// <summary>
        /// do nothing by design. This listener is intended to bu used only with the object method!
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {

        }

        /// <summary>
        /// Used by the Logger class and is already locked so I am sure it is thread safe!
        /// </summary>
        public void Setup()
        {
            if (!IsInitialized)
                Setup(Attributes);
        }
        private void Setup(StringDictionary attributes)
        {
            IsInitialized = true;

            AttributesCopy = attributes;

            Initialize();

            if (null != Buffer)
                ThreadPool.QueueUserWorkItem(ProcessBuffer, Buffer);
            else
            {
                LogMessage("The buffer state info is not initialized for the listener used.");
            }

        }

        protected void ProcessBuffer(object state)
        {
            var bufferInfo = state as MessageBufferListenerStateInfo;
            if (null == bufferInfo)
                return;

            // Force the first iteration!
            bufferInfo.ResetEvent.Set();

            do
            {
                try
                {
                    if (bufferInfo.ResetEvent.WaitOne())
                    {
                        // If timer, disable it.
                        if (null != bufferInfo.Timer)
                        {
                            bufferInfo.Timer.Dispose();
                            bufferInfo.Timer = null;
                        }
                        List<TraceMessage> copy;
                        // I have a message.
                        lock (bufferInfo.Locker)
                        {
                            // Can copy the messages.
                            copy = bufferInfo.Buffer;
                            bufferInfo.Buffer = new List<TraceMessage>();
                        }

                        if (null != copy)
                        {
                            ProcessMessages(copy);
                        }
                    }
                }
                catch (ThreadAbortException) { }
                // Continue processing if problem.
                catch (Exception loggerEx)
                {
                    LogMessage(loggerEx.ToString());
                }
                finally
                {
                    bufferInfo.Timer = new Timer(timerState => bufferInfo.ResetEvent.Set(), null, 500, 500);
                }
            } while (!StopProcessing);

        }

        protected abstract void ProcessMessages(List<TraceMessage> messages);

        protected abstract void Initialize();

        protected void LogMessage(string message)
        {
            // Not possible to log to event viewer.
        }

        protected override void Dispose(bool disposing)
        {
            StopProcessing = true;
            base.Dispose(disposing);
        }
    }
}

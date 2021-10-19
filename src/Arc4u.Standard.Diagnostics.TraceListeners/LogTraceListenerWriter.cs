using System;
using System.Diagnostics;

namespace Arc4u.Diagnostics.Tracelistener
{
    public class LogTraceListenerWriter : ILogWriter
    {
        public void Initialize()
        {
            // Setup all the listeners once because Attributes is filled after the listener is built!
            foreach (var listener in Trace.Listeners)
            {
                try
                {
                    if (listener is TraceListener arc4uListener)
                    {
                        arc4uListener.Setup();
                    }
                }
                catch
                {

                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var listener in Trace.Listeners)
                    {
                        try
                        {
                            if (listener is TraceListener arc4uListener)
                            {
                                arc4uListener.Dispose();
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                disposedValue = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arc4u.Diagnostics.Serilog.Sinks
{
    public interface ILogStore
    {
        List<LogMessage> GetLogs(String criteria, int skip, int take);

        void RemoveAll();
    }
}

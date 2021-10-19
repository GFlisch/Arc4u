using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc4u.Diagnostics.Serilog.Sinks.Memory
{
    public class MemoryLogStore : ILogStore
    {
        public MemoryLogStore(MemoryLogMessages logMessages)
        {
            _logMessages = logMessages;
        }

        private readonly MemoryLogMessages _logMessages;

        public List<LogMessage> GetLogs(string criteria, int skip, int take)
        {
            var hasCriteria = !String.IsNullOrWhiteSpace(criteria);
            var searchText = hasCriteria ? criteria.ToLowerInvariant() : "";

            if (hasCriteria)
                return _logMessages.Where(m => m.Message.Contains(searchText)).Skip(skip).Take(take).ToList();

            return _logMessages.Skip(skip).Take(take).ToList();
        }

        public void RemoveAll()
        {
            _logMessages.Clear();
        }
    }
}

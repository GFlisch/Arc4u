using Microsoft.Extensions.Logging;
using Realms;

namespace Arc4u.Diagnostics.Serilog.Sinks.RealmDb;

public class RealmLoggingDbCtx : ILogStore
{
    public RealmLoggingDbCtx(RealmConfiguration config, ILoggerFactory loggerFactory)
    {
        _realm = Realm.GetInstance(config);
    }

    public void RemoveAll()
    {
        _realm.Write(_realm.RemoveAll<LogDBMessage>);
    }

    public List<LogMessage> GetLogs(string criteria, int skip, int take)
    {
        var hasCriteria = !string.IsNullOrWhiteSpace(criteria);
        var searchText = hasCriteria ? criteria.ToLowerInvariant() : "";

        var queryable = _realm.All<LogDBMessage>().OrderByDescending(msg => msg.Timestamp);

        var enumerator = queryable.GetEnumerator();

        var i = 0;
        while (i < skip && enumerator.MoveNext())
        {
            i++;
        }

        var result = new List<LogDBMessage>(take);
        i = 0;

        while (i < take && enumerator.MoveNext())
        {
            if (hasCriteria && enumerator.Current.Message.ToLowerInvariant().Contains(searchText))
            {
                result.Add(enumerator.Current);
            }

            if (!hasCriteria)
            {
                result.Add(enumerator.Current);
            }

            i++;
        }

        // return the list<LogMessage> based on the list<LogDBMessage>.
        var mapped = result.Select(db => new LogMessage
        {
            Message = db.Message,
            MessageCategory = ((MessageCategory)db.MessageCategory).ToString(),
            MessageType = ((LogLevel)db.MessageType).ToString(),
            Timestamp = db.Timestamp,
            ActivityId = db.ActivityId,
            Application = db.Application,
            Identity = db.Identity,
            ClassType = db.ClassType,
            MethodName = db.MethodName,
            ProcessId = db.ProcessId,
            ThreadId = db.ThreadId,
            Stacktrace = db.Stacktrace,
            Properties = db.Properties
        }).ToList();

        return mapped;
    }

    private readonly Realm _realm;
    private readonly ILoggerFactory _loggerFactory;
    public Realm Realm { get { return _realm; } }
}

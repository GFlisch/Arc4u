using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arc4u.Diagnostics;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arc4u.Standard.UnitTest;

public class LoggingTest
{
    [Fact]
    public void ConcurrentLogs()
    {
        var fix = new Fixture();
        var logger = new MyLogger(); // loggerMock.Object;

        var commonMessageLogger = logger.Technical();
        var logValues = Enumerable
            .Range(1, 999)
            .ToDictionary(i => i, _ => fix.Create<string>())
            .AsParallel()
            .Select(s =>
            {
                commonMessageLogger.Information(s.Value)
                    .Add("Number", s.Key)
                    .Log();
                return s;
            })
            .ToList();


        Assert.Equal(logValues.Count, logger.Messages.Count);


        foreach (var value in logValues)
        {
            var message = logger.Messages.SingleOrDefault(pair => value.Key.Equals(pair.Key.Id));
            Assert.NotEqual(default, message);
            Assert.Equal(value.Value, message.Value);
        }
    }
}

public class MyLogger : ILogger<LoggingTest>
{
    internal ConcurrentDictionary<LogMetaData, string> Messages { get; } = new();

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!(state is Dictionary<string, object> dic))
            return;
        ;
        Messages.TryAdd(new LogMetaData
            {
                Id = dic["Number"],
                Tid = dic["Tid"],
                Pid = dic["Pid"]
            },
            formatter(state, exception));
    }
}

public class LogMetaData
{
    public object Pid { get; set; }
    public object Id { get; set; }
    public object Tid { get; set; }
}
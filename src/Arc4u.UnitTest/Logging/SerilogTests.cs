using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using Arc4u.UnitTest.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class SerilogTests : BaseContainerFixture<SerilogTests, BasicFixture>
{
    public SerilogTests(BasicFixture containerFixture) : base(containerFixture)
    {
    }

    [Fact]
    public async Task LoggerArgumentTest()
    {
        using var container = Fixture.CreateScope();
        LogStartBanner();

        var logger = container.Resolve<ILogger<SerilogTests>>();

        using (new LoggerContext())
        {
            LoggerContext.Current!.Add("Speed", 101011);

            logger!.Technical().LogDebug("Debug {Code}", 100);
            logger!.Technical().LogInformation("Information {Code}", 101);
            logger!.Technical().LogWarning("Warning {Code}", 102);
            logger!.Technical().LogError("Error {Code}", 103);
            logger!.Technical().LogFatal("Fatal {Code}", 104);
            logger!.Technical().LogException(new DivideByZeroException("Cannot divide by zero"));

            await Task.Delay(1000);

            Assert.Equal(1, 1);
        }

        LogEndBanner();
    }

    [Fact]
    public async Task LoggerTechnicalTest()
    {
        using var container = Fixture.CreateScope();
        LogStartBanner();

        var logger = container.Resolve<ILogger<SerilogTests>>();

        using (new LoggerContext())
        {
            LoggerContext.Current!.Add("Speed", 101011);

            logger!.Technical().Debug("Debug").Add("Code", "100").Log();
            logger!.Technical().Information("Information").Add("Code", "101").Log();
            logger!.Technical().Warning("Warning").Add("Code", "102").Log();
            logger!.Technical().Error("Error").Add("Code", "103").Log();
            logger!.Technical().Fatal("Fatal").Add("Code", "104").Log();
            logger!.Technical().Exception(new DivideByZeroException("Cannot divide by zero")).Log();
            logger!.Monitoring().Debug("Memory").AddMemoryUsage().Log();

            await Task.Delay(1000);

            Assert.Equal(1, 1);
        }

        LogEndBanner();
    }

    [Fact]
    public void LoggerContextWithEnlistmentTest()
    {
        using (new LoggerContext())
        {
            LoggerContext.Current!.Add("Code", 100);

            using (new LoggerContext())
            {
                Assert.Equal(100, (int)(LoggerContext.Current.All().First(kv => kv.Key.Equals("Code")).Value));
                LoggerContext.Current.Add("Code2", 101);
                LoggerContext.Current.Add("Code", 101);
                Assert.Contains(LoggerContext.Current.All(), kv => kv.Key.Equals("Code2"));
                Assert.Equal(101, (int)(LoggerContext.Current.All().First(kv => kv.Key.Equals("Code2")).Value));
                Assert.Contains(LoggerContext.Current.All(), kv => kv.Key.Equals("Code"));
            }
            Assert.DoesNotContain(LoggerContext.Current.All(), kv => kv.Key.Equals("Code2"));
            Assert.Contains(LoggerContext.Current.All(), kv => kv.Key.Equals("Code"));
            var value = LoggerContext.Current.All().First(kv => kv.Key.Equals("Code")).Value;
            Assert.Equal(100, (int)value);
        }
    }
}

#region sinks and SerilogWriter
public sealed class AnonymousSinkTest : ILogEventSink, IDisposable
{
    public bool IsAnonymous { get; set; }
    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        IsAnonymous = !logEvent.Properties.ContainsKey(Diagnostics.LoggingConstants.Identity);
    }
}

public sealed class SinkTest : ILogEventSink, IDisposable
{
    public bool Emited { get; set; }
    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        Emited = true;
    }
}

public sealed class FromSinkTest : ILogEventSink, IDisposable
{
    public string Class { get; set; } = default!;

    public string MethodName { get; set; } = default!;

    public string Application { get; set; } = default!;

    public Diagnostics.MessageCategory Category { get; set; }

    public string? ActivityId { get; set; } = default!;

    public IReadOnlyDictionary<string, LogEventPropertyValue> Properties { get; set; } = default!;

    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(LoggingConstants.Class, out var classPropertyValue))
        {
            Class = GetValue(classPropertyValue, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.MethodName, out var methodPropertyValue))
        {
            MethodName = GetValue(methodPropertyValue, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.Category, out var categoryPropertyValue))
        {
            Category = (Diagnostics.MessageCategory)GetValue<short>(categoryPropertyValue, 1);
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.Application, out var applicationName))
        {
            Application = GetValue(applicationName, "")!;
        }

        if (logEvent.Properties.TryGetValue(LoggingConstants.ActivityId, out var activityId))
        {
            ActivityId = GetValue(activityId, "");
        }

        Properties = logEvent.Properties;
    }

    public T? GetValue<T>(LogEventPropertyValue pv, T defaultValue)
    {
        return pv.GetType().Name switch
        {
            nameof(ScalarValue) => (T?)((ScalarValue)pv).Value,
            _ => defaultValue,
        };
    }
}

public class LoggerFromTest : SerilogWriter
{
    public FromSinkTest FromTest { get; set; } = default!;

    public override void Configure(LoggerConfiguration configurator)
    {
        FromTest = new FromSinkTest();

        configurator.WriteTo.Sink(FromTest);
    }
}

public class LoggerFilterSinkTest : SerilogWriter
{
    public SinkTest? Sink { get; set; }

    public override void Configure(LoggerConfiguration configurator)
    {
        Sink = new SinkTest();

        configurator.WriteTo.Sink(Sink);
    }
}

public class LoggerAnonymousSinkTest : SerilogWriter
{
    public AnonymousSinkTest? Sink { get; set; }

    public override void Configure(LoggerConfiguration configurator)
    {
        Sink = new AnonymousSinkTest();

        configurator.WriteTo.Anonymizer(Sink);
    }
}

//public class LoggerTest : SerilogWriter
//{
//    public override void Configure(LoggerConfiguration configurator)
//    {
//        RealmDBExtension.DefaultConfig = () => new RealmConfiguration(@"c:\temp\LoggingDB.realm") { SchemaVersion = 1 };
//        configurator
//            .WriteTo.File(new SimpleTextFormatter(), @"c:\temp\log-.txt"
//                          , rollingInterval: RollingInterval.Minute)
//            .WriteTo.RealmDB();
//    }
//}

public class EmptyLogger : SerilogWriter
{
    public override void Configure(LoggerConfiguration configurator)
    {
    }
}

#endregion sinks and SerilogWriter

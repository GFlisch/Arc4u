using System.Globalization;
using Arc4u.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Arc4u.Dependency;
using FluentAssertions;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class SerilogTests
{
    [Fact]
    public void LoggerArgumentTest()
    {
        var services = new ServiceCollection();

        var sink = new SinkSpeedCategoryTest();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Sink(sink)
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<SerilogTests>>();

        using (new LoggerContext())
        {
            LoggerContext.Current!.Add("Speed", SinkSpeedCategoryTest.Value);

            logger.Technical().LogDebug("Debug {Code}", 100);
            logger.Technical().LogInformation("Information {Code}", 101);
            logger.Technical().LogWarning("Warning {Code}", 102);
            logger.Technical().LogError("Error {Code}", 103);
            logger.Technical().LogFatal("Fatal {Code}", 104);
            logger.Technical().LogException(new DivideByZeroException("Cannot divide by zero"));
            logger!.Monitoring().Debug("Memory").AddMemoryUsage().Log();


            sink.HitCount.Should().Be(7);
            sink.Emited.Should().BeTrue();
        }
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

public sealed class SinkSpeedCategoryTest : ILogEventSink
{
    public const int Value = 101011;
    public bool Emited { get; set; }

    public uint HitCount { get; private set; }

    public void Emit(LogEvent logEvent)
    {
        if (Emited || HitCount == 0)
        {
            Emited = logEvent.Properties.TryGetValue("Speed", out var value) && value.ToString() == Value.ToString(CultureInfo.InvariantCulture);
        }

        HitCount++;
    }
}

public sealed class SinkTest : ILogEventSink
{
    public bool Emited { get; set; }

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
            Category = (MessageCategory)Enum.Parse(typeof(MessageCategory), GetValue(categoryPropertyValue, MessageCategory.Technical.ToString())!);
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

#endregion sinks and SerilogWriter
using Arc4u.Results.Logging;
using Arc4u.Results.Validation;
using Arc4u.Security.Principal;
using AwesomeAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arc4u.UnitTest.Results;

[Trait("Category", "CI")]
public class FluentLoggerTests
{
    // Wires the logger the way a host does: AddApplicationContext() swaps ILogger<> for the
    // Arc4u wrapper that FluentLogger needs, and Result.Setup routes the reasons to it.
    private static List<(LogLevel Level, string Message)> Capture(Action<IResultLogger> act)
    {
        var captured = new List<(LogLevel, string)>();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace)
                                              .AddProvider(new CapturingProvider(captured)));
        services.AddApplicationContext();
        services.AddSingleton<IResultLogger, FluentLogger>();

        using var provider = services.BuildServiceProvider();

        act(provider.GetRequiredService<IResultLogger>());

        return captured;
    }

    [Fact]
    public void Log_A_Single_ValidationError_Should_Log_It_Once()
    {
        var result = Result.Fail(ValidationError.Create("name is required"));

        var captured = Capture(logger => logger.Log(string.Empty, string.Empty, result, LogLevel.Error));

        captured.Should().ContainSingle();
        captured[0].Level.Should().Be(LogLevel.Error);
        captured[0].Message.Should().Be("name is required");
    }

    [Fact]
    public void Log_Several_ValidationErrors_Should_Log_Each_Of_Them_Once()
    {
        var result = Result.Fail(ValidationError.Create("e1"))
                           .WithError(ValidationError.Create("e2"))
                           .WithError(ValidationError.Create("e3"));

        var captured = Capture(logger => logger.Log(string.Empty, string.Empty, result, LogLevel.Error));

        captured.Should().HaveCount(3);
        captured.Should().OnlyContain(entry => entry.Level == LogLevel.Error);
        captured.Select(entry => entry.Message).Should().BeEquivalentTo("e1", "e2", "e3");
    }

    [Fact]
    public void Log_A_ValidationError_Should_Keep_The_Other_Reasons()
    {
        var result = Result.Fail(ValidationError.Create("name is required"))
                           .WithSuccess("tenant A");

        var captured = Capture(logger => logger.Log(string.Empty, string.Empty, result, LogLevel.Error));

        captured.Should().HaveCount(2);
        captured.Should().ContainSingle(entry => entry.Level == LogLevel.Error && entry.Message == "name is required");
        captured.Should().ContainSingle(entry => entry.Level == LogLevel.Information && entry.Message == "tenant A");
    }

    [Fact]
    public void Log_A_Successful_Result_Should_Log_Its_Reasons_Once()
    {
        var result = Result.Ok().WithSuccess("created").WithSuccess("cached");

        var captured = Capture(logger => logger.Log(string.Empty, string.Empty, result, LogLevel.Information));

        captured.Should().HaveCount(2);
        captured.Select(entry => entry.Message).Should().BeEquivalentTo("created", "cached");
    }

    [Fact]
    public void Log_With_A_Context_Should_Log_Each_ValidationError_Once()
    {
        var result = Result.Fail(ValidationError.Create("e1"))
                           .WithError(ValidationError.Create("e2"));

        var captured = Capture(logger => logger.Log<FluentLoggerTests>("use case failed", result, LogLevel.Error));

        captured.Should().HaveCount(3);
        captured[0].Message.Should().Be("use case failed");
        captured.Skip(1).Select(entry => entry.Message).Should().BeEquivalentTo("e1", "e2");
    }

    private sealed class CapturingProvider(List<(LogLevel, string)> sink) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(sink);

        public void Dispose() { }

        private sealed class CapturingLogger(List<(LogLevel, string)> sink) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => sink.Add((logLevel, formatter(state, exception)));
        }
    }
}

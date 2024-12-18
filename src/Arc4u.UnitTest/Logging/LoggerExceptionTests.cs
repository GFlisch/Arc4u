using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Arc4u.Dependency;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class LoggerSimpleExceptionTests 
{
    [Fact]
    public void ExceptionTest()
    {
        var services = new ServiceCollection();

        var sink = new ExceptionSinkTest();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Sink(sink)
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<LoggerSimpleExceptionTests>>()!;

        logger.Technical().Exception(new StackOverflowException("Overflow", new DivideByZeroException())).Log();

        Assert.True(sink.HasException);
        Assert.Single(sink.Exceptions);
        Assert.IsType<StackOverflowException>(sink.Exceptions.First());
        Assert.IsType<DivideByZeroException>(sink.Exceptions.First().InnerException);
    }
}

[Trait("Category", "CI")]
public class LoggerAggregateExceptionTests
{

    [Fact]
    public void TestAggregateException()
    {
        var services = new ServiceCollection();

        var sink = new ExceptionSinkTest();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Sink(sink)
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<LoggerAggregateExceptionTests>>()!;

        logger.Technical().Exception(new AggregateException("Aggregated",
                                        new DivideByZeroException("Go back to school", new OutOfMemoryException("Out of memory")),
                                        new ArgumentNullException("null"),
                                        new AggregateException(new AppDomainUnloadedException("Houston, we have a problem."))))
                          .Log();

        Assert.True(sink.HasException);
        Assert.Collection(sink.Exceptions,
                                e => Assert.IsType<AggregateException>(e),
                                e => Assert.IsType<DivideByZeroException>(e),
                                e => Assert.IsType<ArgumentNullException>(e),
                                e => Assert.IsType<AppDomainUnloadedException>(e));

        Assert.IsType<OutOfMemoryException>(sink.Exceptions[1].InnerException);

    }
}

public class LoggerExceptionSinkTest : SerilogWriter
{
    public ExceptionSinkTest? Sink { get; set; }

    public override void Configure(LoggerConfiguration configurator)
    {
        Sink = new ExceptionSinkTest();

        configurator.WriteTo.Sink(Sink);
    }
}

public sealed class ExceptionSinkTest : ILogEventSink, IDisposable
{
    public ExceptionSinkTest()
    {
        Exceptions = [];
    }
    public bool HasException { get; set; }

    public List<Exception> Exceptions { get; set; }

    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        HasException = null != logEvent.Exception;
        if (HasException)
        {
            Exceptions.Add(logEvent.Exception!);
        }
    }
}


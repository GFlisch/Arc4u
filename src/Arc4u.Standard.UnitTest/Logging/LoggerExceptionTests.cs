using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class LoggerSimpleExceptionTests : BaseSinkContainerFixture<CategorySerilogTesters, ExceptionFixture>
{
    public LoggerSimpleExceptionTests(ExceptionFixture containerFixture) : base(containerFixture)
    {
    }

    [Fact]
    public void ExceptionTest()
    {
        using var container = Fixture.CreateScope();
        var logger = container.Resolve<ILogger<LoggerSimpleExceptionTests>>();

        var sink = (ExceptionSinkTest)Fixture.Sink;

        logger.Technical().Exception(new StackOverflowException("Overflow", new DivideByZeroException())).Log();

        Assert.True(sink.HasException);
        Assert.Single(sink.Exceptions);
        Assert.IsType<StackOverflowException>(sink.Exceptions.First());
        Assert.IsType<DivideByZeroException>(sink.Exceptions.First().InnerException);
    }
}

[Trait("Category", "CI")]
public class LoggerAggregateExceptionTests : BaseSinkContainerFixture<CategorySerilogTesters, ExceptionFixture>
{
    public LoggerAggregateExceptionTests(ExceptionFixture containerFixture) : base(containerFixture)
    {
    }

    [Fact]
    public void TestAggregateException()
    {
        using var container = Fixture.CreateScope();
        var logger = container.Resolve<ILogger<LoggerAggregateExceptionTests>>();

        var sink = (ExceptionSinkTest)Fixture.Sink;

        logger.Technical().Exception(new AggregateException("Aggregated",
                                        new DivideByZeroException("Go back to school", new OutOfMemoryException("Out of memory")),
                                        new ArgumentNullException("null"),
                                        new AggregateException(
                                            new AppDomainUnloadedException("Houston, we have a problem.")))).Log();

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
    public ExceptionSinkTest Sink { get; set; }

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
        Exceptions = new List<Exception>();
    }
    public bool HasException { get; set; }

    public List<Exception> Exceptions { get; set; }

    public void Dispose()
    {
    }

    public void Emit(LogEvent logEvent)
    {
        HasException = null != logEvent.Exception;
        Exceptions.Add(logEvent.Exception);
    }
}

public class ExceptionFixture : SinkContainerFixture
{
    protected override Serilog.Core.Logger BuildLogger(IConfiguration configuration)
    {
        _sink = new LoggerExceptionSinkTest();
        _sink.Initialize();

        return _sink.Logger;
    }

    private LoggerExceptionSinkTest _sink;

    public override ExceptionSinkTest Sink => _sink.Sink;

    public override string ConfigFile => @"Configs\Basic.json";
}


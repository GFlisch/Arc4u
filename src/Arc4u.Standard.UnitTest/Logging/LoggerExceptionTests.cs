using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Arc4u.Standard.UnitTest.Logging
{
    public class LoggerExceptionTests : BaseSinkContainerFixture<CategorySerilogTesters, ExceptionFixture>
    {
        public LoggerExceptionTests(ExceptionFixture containerFixture) : base(containerFixture)
        {
        }

        [Fact]
        public void ExceptionTest()
        {
            using (var container = Fixture.CreateScope())
            {
                var logger = container.Resolve<ILogger<LoggerExceptionTests>>();

                var sink = (ExceptionSinkTest)Fixture.Sink;

                logger.Technical().Exception(new StackOverflowException("Overflow", new DivideByZeroException())).Log();

                Assert.True(sink.HasException);
                Assert.Equal(typeof(StackOverflowException), sink.Exception.GetType());
                Assert.Equal(typeof(DivideByZeroException), sink.InnerException.GetType());
            }
        }

        [Fact]
        public void TestAggregateException()
        {
            using (var container = Fixture.CreateScope())
            {
                var logger = container.Resolve<ILogger<LoggerExceptionTests>>();

                var sink = (ExceptionSinkTest)Fixture.Sink;

                logger.Technical().Exception(new AggregateException("Aggregated", new DivideByZeroException("Go back to school", new AppDomainUnloadedException("Houston, we have a problem.")))).Log();

                Assert.True(sink.HasException);
                Assert.Equal(typeof(DivideByZeroException), sink.Exception.GetType());
                Assert.Equal(typeof(AppDomainUnloadedException), sink.InnerException.GetType());
            }

        }

        [Fact]
        public void TestInException()
        {
            using (var container = Fixture.CreateScope())
            {
                var logger = container.Resolve<ILogger<LoggerExceptionTests>>();

                var sink = (ExceptionSinkTest)Fixture.Sink;

                logger.Technical().Exception(new StackOverflowException("Overflow", new DivideByZeroException("Go back to school", new AppDomainUnloadedException("Houston, we have a problem.")))).Log();

                Assert.True(sink.HasException);
                Assert.Equal(typeof(StackOverflowException), sink.Exception.GetType());
                Assert.Equal(typeof(DivideByZeroException), sink.InnerException.GetType());
                Assert.Equal(typeof(AppDomainUnloadedException), sink.InnerException.InnerException.GetType());
            }

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
        public bool HasException { get; set; }
        public Exception InnerException { get; set; }

        public Exception Exception { get; set; }

        public void Dispose()
        {
        }

        public void Emit(LogEvent logEvent)
        {
            HasException = null != logEvent.Exception;
             Exception = logEvent.Exception;
            InnerException = logEvent.Exception?.InnerException;
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
}


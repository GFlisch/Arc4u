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
        public async Task ExceptionTest()
        {
            using (var container = Fixture.CreateScope())
            {
                LogStartBanner();

                var logger = container.Resolve<ILogger<LoggerExceptionTests>>();

                var sink = (ExceptionSinkTest)Fixture.Sink;

                logger.Technical().Exception(new StackOverflowException("Overflow", new DivideByZeroException())).Log();

                Assert.True(sink.HasException);
                Assert.Equal(typeof(DivideByZeroException), sink.InnerException);

                LogEndBanner();
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
        public Type InnerException { get; set; }

        public void Dispose()
        {
        }

        public void Emit(LogEvent logEvent)
        {
            HasException = null != logEvent.Exception;

            InnerException = logEvent.Exception?.InnerException?.GetType();
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


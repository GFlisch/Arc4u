using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;
using Xunit;

namespace Arc4u.Standard.UnitTest.Logging
{
    public class CategorySerilogTesters : BaseSinkContainerFixture<CategorySerilogTesters, LoggerCategoryFixture>
    {
        public CategorySerilogTesters(LoggerCategoryFixture containerFixture) : base(containerFixture)
        {
        }

        [Fact]
        public void LoggerTechnicalTest()
        {
            using (var container = Fixture.CreateScope())
            {
                var logger = container.Resolve<ILogger<CategorySerilogTesters>>();

                ((SinkTest)Fixture.Sink).Emited = false;
                logger.Technical().Debug("Technical").Add("Code", "100").Log();
                Assert.False(((SinkTest)Fixture.Sink).Emited);

                Logger.Business().Debug("Business").Add("Code", "100").Log();
                Assert.True(((SinkTest)Fixture.Sink).Emited);
                ((SinkTest)Fixture.Sink).Emited = false;

                Logger.Monitoring().Debug("Monitoring").AddMemoryUsage().Log();
                Assert.True(((SinkTest)Fixture.Sink).Emited);
            }
        }

        [Fact]
        public async Task LoggerSystemResourcesTest()
        {
            using (var container = Fixture.CreateScope())
            {
                var logger = container.Resolve<ILogger<CategorySerilogTesters>>();

                var sink = (SinkTest)Fixture.Sink;

                logger.Monitoring().Information("Message monitoring").Add("Code", 100).Log();

                Assert.True(sink.Emited);
                sink.Emited = false;

                using var monitoring = new Diagnostics.Monitoring.SystemResources(logger, 1)
                {
                    StartMonitoringDelayInSeconds = 1
                };

                await monitoring.StartAsync(new System.Threading.CancellationToken());

                await Task.Delay(1500);

                await monitoring.StopAsync(new System.Threading.CancellationToken());

                Assert.True(sink.Emited);
            }
        }
    }

    public class LoggerCategorySinkTest : SerilogWriter
    {
        public SinkTest Sink { get; set; }

        public FromSinkTest FromTest { get; set; }

        public override void Configure(LoggerConfiguration configurator)
        {
            Sink = new SinkTest();
            FromTest = new FromSinkTest();

            configurator.WriteTo.CategoryFilter(Diagnostics.MessageCategory.Business | Diagnostics.MessageCategory.Monitoring, Sink);
            configurator.WriteTo.Sink(FromTest);
        }
    }
}

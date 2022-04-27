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
        public async Task LoggerTechnicalTest()
        {
            using (var container = Fixture.CreateScope())
            {
                LogStartBanner();

                var logger = container.Resolve<ILogger<CategorySerilogTesters>>();

                ((SinkTest)Fixture.Sink).Emited = false;
                logger.Technical().Debug("Technical").Add("Code", "100").Log();
                Assert.False(((SinkTest)Fixture.Sink).Emited);

                Logger.Business().Debug("Business").Add("Code", "100").Log();
                Assert.True(((SinkTest)Fixture.Sink).Emited);
                ((SinkTest)Fixture.Sink).Emited = false;

                Logger.Monitoring().Debug("Monitoring").AddMemoryUsage().Log();
                Assert.True(((SinkTest)Fixture.Sink).Emited);

                LogEndBanner();
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

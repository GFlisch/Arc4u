using Arc4u.Diagnostics;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class NullLoggerRapperTests
{
    public NullLoggerRapperTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }
    private readonly Fixture _fixture;

    [Fact]
    public void NullLoggetWrapper_Shoud()
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());


        var loggerWrapper = NullLoggerWrapper<CategorySerilogTesters>.Instance;
        fixture.Inject<ILogger<CategorySerilogTesters>>(loggerWrapper);

        var logger = fixture.Create<ILogger<CategorySerilogTesters>>();
        Assert.NotNull(logger.Technical());
        Assert.NotNull(logger.Business());
        Assert.NotNull(logger.Monitoring());
        logger.Technical().LogDebug("Test");
        logger.Technical().LogInformation("Test");
        logger.Technical().LogWarning("Test");
        logger.Technical().LogError("Test");
        logger.Technical().LogCritical("Test");

        logger.Technical().Add("key", "value").LogTrace("Test");

    }
}

using Arc4u.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Environment = Arc4u.Configuration.Environment;

namespace Arc4u.Standard.UnitTest.Core;

public class TimeZoneContextTests
{
    private readonly Mock<IOptionsMonitor<ApplicationConfig>> _mockConfig;
    private readonly Mock<ILogger> _mockLogger;
    private readonly ApplicationConfig _appConfig;

    public TimeZoneContextTests()
    {
        _mockConfig = new Mock<IOptionsMonitor<ApplicationConfig>>();
        _mockLogger = new Mock<ILogger>();
        _appConfig = new ApplicationConfig
        {
            ApplicationName = "TestApp",
            Environment = new Environment
            {
                Name = "Development",
                LoggingName = "TestApp",
                TimeZone = "Pacific Standard Time"
            }
        };

        _mockConfig.Setup(c => c.CurrentValue).Returns(_appConfig);
    }

    [Fact]
    public void InitializeTimeZoneContext_ShouldSetTimeZone()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfig.Object, _mockLogger.Object);

        // Act
        var timeZoneInfo = timeZoneContext.TimeZoneInfo;

        // Assert
        Assert.Equal("Pacific Standard Time", timeZoneInfo.Id);
    }

    [Fact]
    public void ConvertFromUtc_ShouldConvertToSpecifiedTimeZone()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfig.Object, _mockLogger.Object);
        var utcTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var localTime = timeZoneContext.ConvertFromUtc(utcTime);

        // Assert
        Assert.Equal(DateTimeKind.Local, localTime.Kind);
        Assert.Equal(utcTime.AddHours(-8), localTime); // Pacific Standard Time is UTC-8
    }

    [Fact]
    public void ConvertToUtc_ShouldConvertToUtc()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfig.Object, _mockLogger.Object);
        var localTime = new DateTime(2023, 1, 1, 4, 0, 0, DateTimeKind.Local);

        // Act
        var utcTime = timeZoneContext.ConvertToUtc(localTime);

        // Assert
        Assert.Equal(DateTimeKind.Utc, utcTime.Kind);
        Assert.Equal(localTime.AddHours(8), utcTime); // Pacific Standard Time is UTC-8
    }

    [Fact]
    public void GetDaylightChanges_ShouldReturnDaylightTime()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfig.Object, _mockLogger.Object);

        // Act
        var daylightChanges = timeZoneContext.GetDaylightChanges(2023);

        // Assert
        Assert.NotNull(daylightChanges);
    }
}

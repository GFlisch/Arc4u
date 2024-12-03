using Arc4u.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Environment = Arc4u.Configuration.Environment;

namespace Arc4u.UnitTest.Core;

public class TimeZoneContextTests
{
    private readonly Mock<IOptionsMonitor<ApplicationConfig>> _mockConfigPST;
    private readonly Mock<IOptionsMonitor<ApplicationConfig>> _mockConfigBE;
    private readonly Mock<ILogger> _mockLogger;
    private readonly ApplicationConfig _appConfigPST;
    private readonly ApplicationConfig _appConfigBE;

    public TimeZoneContextTests()
    {
        _mockConfigPST = new Mock<IOptionsMonitor<ApplicationConfig>>();
        _mockConfigBE = new Mock<IOptionsMonitor<ApplicationConfig>>();
        _mockLogger = new Mock<ILogger>();
        _appConfigPST = new ApplicationConfig
        {
            ApplicationName = "TestApp",
            Environment = new Environment
            {
                Name = "Development",
                LoggingName = "TestApp",
                TimeZone = "Pacific Standard Time"
            }
        };

        _appConfigBE = new ApplicationConfig
        {
            ApplicationName = "TestApp",
            Environment = new Environment
            {
                Name = "Development",
                LoggingName = "TestApp",
                TimeZone = "Romance Standard Time"
            }
        };

        _mockConfigPST.Setup(c => c.CurrentValue).Returns(_appConfigPST);
        _mockConfigBE.Setup(c => c.CurrentValue).Returns(_appConfigBE);
    }

    [Fact]
    public void InitializeTimeZoneContext_ShouldSetTimeZone()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfigPST.Object, _mockLogger.Object);

        // Act
        var timeZoneInfo = timeZoneContext.TimeZoneInfo;

        // Assert
        Assert.Equal("Pacific Standard Time", timeZoneInfo.Id);
    }

    [Fact]
    public void ConvertFromUtc_ShouldConvertToSpecifiedTimeZone()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfigPST.Object, _mockLogger.Object);
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
        var timeZoneContextPST = new TimeZoneContext(_mockConfigPST.Object, _mockLogger.Object);
        var timeZoneContextBE = new TimeZoneContext(_mockConfigBE.Object, _mockLogger.Object);

        var utcTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var localTime = timeZoneContextBE.ConvertFromUtc(utcTime);

        // Act
        var pstUtcTime = timeZoneContextPST.ConvertToUtc(localTime);

        // Assert
        Assert.Equal(DateTimeKind.Utc, utcTime.Kind);
        Assert.Equal(pstUtcTime, utcTime);
    }

    [Fact]
    public void GetDaylightChanges_ShouldReturnDaylightTime()
    {
        // Arrange
        var timeZoneContext = new TimeZoneContext(_mockConfigPST.Object, _mockLogger.Object);

        // Act
        var daylightChanges = timeZoneContext.GetDaylightChanges(2023);

        // Assert
        Assert.NotNull(daylightChanges);
    }

    [Fact]
    public void GetDaylightChanges_NoAdjustmentRules_ReturnsNull()
    {
        // Arrange
        var timeZoneContextBE = new TimeZoneContext(_mockConfigBE.Object, _mockLogger.Object);

        var timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("TestZone", TimeSpan.Zero, "TestZone", "TestZone", "TestZone", []);
        timeZoneContextBE.GetType().GetField("_timeZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(timeZoneContextBE, timeZoneInfo);

        // Act
        var result = timeZoneContextBE.GetDaylightChanges(2020);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDaylightChanges_AdjustmentRulesExist_ReturnsDaylightTime()
    {
        // Arrange
        var adjustmentRules = new[]
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                new DateTime(2023, 3, 10),
                new DateTime(2023, 11, 3),
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 10),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 2, 0, 0), 11, 3)
                )
        };

        var timeZoneContextBE = new TimeZoneContext(_mockConfigBE.Object, _mockLogger.Object);

        var timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("TestZone", TimeSpan.Zero, "TestZone", "TestZone", "TestZone", adjustmentRules);
        timeZoneContextBE.GetType().GetField("_timeZone", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(timeZoneContextBE, timeZoneInfo);

        // Act
        var result = timeZoneContextBE.GetDaylightChanges(2023);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new DateTime(2023, 3, 5, 2, 0, 0).AsLocalTime(), result.Start);
        Assert.Equal(new DateTime(2023, 11, 5, 2, 0, 0).AsLocalTime(), result.End);
        Assert.Equal(TimeSpan.FromHours(1), result.Delta);
    }
}

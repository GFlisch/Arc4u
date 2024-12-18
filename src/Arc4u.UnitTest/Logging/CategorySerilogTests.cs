using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Serilog;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class CategorySerilogTesters 
{
    [Fact]
    public void LoggerTechnicalTest()
    {
        var services = new ServiceCollection();

        var sink = new SinkTest();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Sink(sink)
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetService<ILogger<CategorySerilogTesters>>()!;

        sink.Emited = false;
        logger.Technical().Debug("Technical").Add("Code", "100").Log();
        sink.Emited.Should().BeTrue();

        logger.Business().Debug("Business").Add("Code", "100").Log();
        sink.Emited.Should().BeTrue();
        sink.Emited = false;

        logger.Monitoring().Debug("Monitoring").AddMemoryUsage().Log();
        sink.Emited.Should().BeTrue();
    }

    [Fact]
    public async Task LoggerSystemResourcesTest()
    {
        // Register Serilog.
        var services = new ServiceCollection();

        var sink = new SinkTest();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Sink(sink)
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetService<ILogger<CategorySerilogTesters>>()!;

        logger.Monitoring().Information("Message monitoring").Add("Code", 100).Log();

        Assert.True(sink.Emited);
        sink.Emited = false;

        using var monitoring = new Diagnostics.Monitoring.SystemResources(logger, 1)
        {
            StartMonitoringDelayInSeconds = 1
        };

        await monitoring.StartAsync(new CancellationToken());

        await Task.Delay(1500);

        await monitoring.StopAsync(new CancellationToken());

        Assert.True(sink.Emited);
    }
}

public class LoggerCategorySinkTest : SerilogWriter
{
    public SinkTest? Sink { get; set; }

    public FromSinkTest? FromTest { get; set; }

    public override void Configure(LoggerConfiguration configurator)
    {
        Sink = new SinkTest();
        FromTest = new FromSinkTest();

        configurator.WriteTo.CategoryFilter(Diagnostics.MessageCategory.Business | Diagnostics.MessageCategory.Monitoring, Sink);
        configurator.WriteTo.Sink(FromTest);
    }
}

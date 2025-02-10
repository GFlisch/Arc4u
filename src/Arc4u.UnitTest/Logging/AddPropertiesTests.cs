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
public class AddPropertiesTests
{
    [Fact]
    public void Test_AddProperties_With_Double_DI_Registrations()
    {
        var services = new ServiceCollection();
        var serilog = new LoggerConfiguration()
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();
        services.AddSingleton<IAddPropertiesToLog, NullLoggerProperties>();
        services.AddScoped<IAddPropertiesToLog, NullLoggerProperties>();
        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<AddPropertiesTests>>()!;
    }
}

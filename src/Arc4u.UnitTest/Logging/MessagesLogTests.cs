using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;
using Arc4u.Dependency;
using Arc4u.ServiceModel;
using MessageCategory = Arc4u.ServiceModel.MessageCategory;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class MessagesLogTests
{
    [Fact]
    public void TestLogAll()
    {
        var services = new ServiceCollection();

        var serilog = new LoggerConfiguration()
                             .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                             .MinimumLevel.Debug()
                             .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: serilog, dispose: false));
        services.AddILogger();

        var serviceProvider = services.BuildServiceProvider();

        var messages = new Messages
        {
            new Message(MessageCategory.Technical, MessageType.Error, "An error message.")
        };

        var logger = serviceProvider.GetRequiredService<ILogger<MessagesLogTests>>()!;

        messages.LogAll(logger);
    }
}

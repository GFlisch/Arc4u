using Arc4u.ServiceModel;
using Arc4u.UnitTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Arc4u.UnitTest.Logging;

[Trait("Category", "CI")]
public class MessagesLogTests : BaseContainerFixture<MessagesLogTests, BasicFixture>
{
    public MessagesLogTests(BasicFixture containerFixture) : base(containerFixture)
    {
    }

    [Fact]
    public void TestLogAll()
    {
        var container = Fixture.CreateScope();
        LogStartBanner();

        var messages = new Messages
        {
            new Message(MessageCategory.Technical, MessageType.Error, "An error message.")
        };

        var logger = container.GetRequiredService<ILogger<MessagesLogTests>>()!;

        messages.LogAll(logger);

        LogEndBanner();
    }
}

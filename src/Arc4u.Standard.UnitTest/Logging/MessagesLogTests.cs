using Arc4u.ServiceModel;
using Arc4u.UnitTest.Infrastructure;
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
        using var container = Fixture.CreateScope();
        LogStartBanner();

        var messages = new Messages();
        messages.Add(new Message(MessageCategory.Technical, MessageType.Error, "An error message."));

        var logger = container.Resolve<ILogger<MessagesLogTests>>();

        messages.LogAll(logger);

        LogEndBanner();
    }
}

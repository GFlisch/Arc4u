using Arc4u.ServiceModel;
using Arc4u.Standard.UnitTest.Infrastructure;
using Arc4u.Standard.UnitTest.KubeMQ;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Arc4u.Standard.UnitTest.Logging
{
    public class MessagesLogTests : BaseContainerFixture<MessagesLogTests, BasicFixture>
    {
        public MessagesLogTests(BasicFixture containerFixture) : base(containerFixture)
        {
        }

        [Fact]
        public void TestLogAll()
        {
            using (var container = Fixture.CreateScope())
            {
                LogStartBanner();

                var messages = new Messages();
                messages.Add(new Message(MessageCategory.Technical, MessageType.Error, "An error message."));

                var logger = container.Resolve<ILogger<MessagesLogTests>>();

                messages.LogAll(logger);

                LogEndBanner();
            }
        }
    }
}

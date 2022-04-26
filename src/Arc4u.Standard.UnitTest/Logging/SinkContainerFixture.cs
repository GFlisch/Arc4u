using Arc4u.Standard.UnitTest.Infrastructure;
using Serilog.Core;

namespace Arc4u.Standard.UnitTest.Logging
{
    public abstract class SinkContainerFixture : ContainerFixture, ISinkContainerFixture
    {

        public abstract ILogEventSink Sink { get; }


    }
}

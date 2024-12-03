using Arc4u.UnitTest.Infrastructure;
using Serilog.Core;

namespace Arc4u.UnitTest.Logging;

public abstract class SinkContainerFixture : ContainerFixture, ISinkContainerFixture
{

    public abstract ILogEventSink Sink { get; }

}

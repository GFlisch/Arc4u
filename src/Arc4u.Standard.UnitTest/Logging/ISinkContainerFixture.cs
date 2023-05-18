using Arc4u.UnitTest.Infrastructure;
using Serilog.Core;

namespace Arc4u.UnitTest.Logging
{
    public interface ISinkContainerFixture : IContainerFixture
    {
        public ILogEventSink Sink { get; }
    }
}

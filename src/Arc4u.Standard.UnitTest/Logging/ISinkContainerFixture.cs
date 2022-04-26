using Arc4u.Standard.UnitTest.Infrastructure;
using Serilog.Core;

namespace Arc4u.Standard.UnitTest.Logging
{
    public interface ISinkContainerFixture : IContainerFixture
    {
        public ILogEventSink Sink { get; }
    }
}

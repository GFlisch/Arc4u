using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Runtime.CompilerServices;
using Xunit;

namespace Arc4u.Standard.UnitTest.Logging
{
    public abstract class BaseSinkContainerFixture<T, TFixture> : IClassFixture<TFixture> where TFixture : class, ISinkContainerFixture
    {
        protected readonly ISinkContainerFixture _containerFixture;
        private readonly ILogger<T> _logger;

        public BaseSinkContainerFixture(TFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _logger = containerFixture.SharedContainer.Resolve<ILogger<T>>();
        }

        public ILogger<T> Logger => _logger;

        public ILogEventSink Sink => _containerFixture.Sink;

        public ISinkContainerFixture Fixture => _containerFixture;

        public virtual void LogStartBanner([CallerMemberName] string methodName = "")
        {
            _logger.Technical().Debug($"**** Start testing of {methodName} ****").Log();
        }

        public virtual void LogEndBanner([CallerMemberName] string methodName = "")
        {
            _logger.Technical().Debug($"**** End testing of {methodName} ****").Log();
        }
    }
}

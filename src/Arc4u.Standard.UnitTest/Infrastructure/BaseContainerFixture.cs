using Arc4u.Diagnostics;
using Arc4u.UnitTest.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Xunit;

namespace Arc4u.UnitTest.Infrastructure
{
    public abstract class BaseContainerFixture<T, TFixture> : IClassFixture<TFixture> where TFixture : class, IContainerFixture
    {
        protected readonly IContainerFixture _containerFixture;
        private readonly ILogger<T> _logger;

        public BaseContainerFixture(TFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _logger = containerFixture.SharedContainer.Resolve<ILogger<T>>();
        }

        public ILogger<T> Logger => _logger;

        public IContainerFixture Fixture => _containerFixture;

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

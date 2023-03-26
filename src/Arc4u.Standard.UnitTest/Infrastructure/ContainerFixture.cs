using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Dependency.ComponentModel;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Arc4u.Standard.UnitTest.Infrastructure
{
    public abstract class ContainerFixture : IDisposable, IContainerFixture
    {
        public ContainerFixture()
        {
            var configuration = ConfigurationHelper.GetConfigurationFromFile(ConfigFile);

            var services = new ServiceCollection();
            services.AddApplicationConfig(configuration);

            _logger = BuildLogger(configuration);

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger: _logger, dispose: false));
            services.AddILogger();

            services.AddApplicationContext();

            AddServices(services, configuration);

            var container = new ComponentModelContainer(services).InitializeFromConfig(configuration);
            container.RegisterInstance(configuration);

            AddToContainer(container, configuration);

            container.CreateContainer();

            Container = container;
        }

        private readonly Logger _logger;

        protected virtual Logger BuildLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                                 .ReadFrom.Configuration(configuration)
                                 .CreateLogger();
        }

        protected virtual void AddServices(IServiceCollection services, IConfiguration configuration) { }

        protected virtual void AddToContainer(IContainerRegistry containerRegistry, IConfiguration configuration) { }

        public abstract string ConfigFile { get; }

        public virtual AppPrincipal GetPrincipal()
        {
            var authorization = new Authorization
            {
                AllOperations = new(),
                Roles = new List<ScopedRoles> { new ScopedRoles { Scope = "", Roles = new List<string> { "Admin" } } },
                Scopes = new List<string> { "" },
                Operations = new List<ScopedOperations> { new ScopedOperations { Operations = new() { 1 }, Scope = "" } }
            };

            authorization.AllOperations.Add(new Operation { ID = 1, Name = "AccessApplication" });

            return new AppPrincipal(authorization, new GenericIdentity("TestUser"), "S-1-9-5-100") { ActivityID = Guid.NewGuid() };

        }

        private IContainerResolve Container { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        public IContainerResolve CreateScope() => Container.CreateScope();

        public IContainerResolve SharedContainer => Container;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger.Dispose();
                    // TODO: dispose managed state (managed objects).
                    Container.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}

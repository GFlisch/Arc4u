using Arc4u.Dependency;
using Arc4u.Standard.UnitTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Arc4u.Standard.UnitTest.Database.EfCore
{
    public class EfCoreFixture : ContainerFixture
    {

        public override string ConfigFile => @"Configs\EfCore.json";

        protected override void AddToContainer(IContainerRegistry containerRegistry, IConfiguration configuration)
        {
            containerRegistry.RegisterSingletonFactory(() =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

                optionsBuilder.UseInMemoryDatabase("EfCore")
                              .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                return optionsBuilder.Options;
            });

            containerRegistry.RegisterScoped<DatabaseContext, DatabaseContext>();
        }
    }
}

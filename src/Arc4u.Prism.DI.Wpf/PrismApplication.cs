using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommonServiceLocator;
using Prism.DI.Regions;
using Prism.Ioc;
using Prism.Regions;
using Prism.Regions.Behaviors;

namespace Prism.DI
{
    public abstract class PrismApplication : PrismApplicationBase
    {

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {

            if (regionAdapterMappings != null)
            {
                regionAdapterMappings.RegisterMapping(typeof(Selector), Container.Resolve<DISelectorRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ItemsControl), Container.Resolve<DIItemsControlRegionAdapter>());
                regionAdapterMappings.RegisterMapping(typeof(ContentControl), Container.Resolve<DIContentControlRegionAdapter>());
            }
        }

        protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterRequiredTypes(containerRegistry);
            containerRegistry.Register<AutoPopulateRegionBehavior>();
            containerRegistry.Register<ClearChildViewsRegionBehavior>();
            containerRegistry.Register<IDestructibleRegionBehavior>();
            containerRegistry.Register<SelectorItemsSourceSyncBehavior>();
            containerRegistry.Register<RegionMemberLifetimeBehavior>();
            containerRegistry.Register<RegionManagerRegistrationBehavior>();
            containerRegistry.Register<SyncRegionContextWithHostBehavior>();
            containerRegistry.Register<RegionActiveAwareBehavior>();
            containerRegistry.Register<BindRegionContextToDependencyObjectBehavior, DIBindRegionContextToDependencyObjectBehavior>();
            containerRegistry.Register<DelayedRegionCreationBehavior, DIDelayedRegionCreationBehavior>();
            containerRegistry.Register<DISelectorRegionAdapter>();
            containerRegistry.Register<DIItemsControlRegionAdapter>();
            containerRegistry.Register<DIContentControlRegionAdapter>();
            containerRegistry.RegisterSingleton<IRegionNavigationContentLoader, RegionNavigationContentLoader>();
            containerRegistry.RegisterSingleton<IServiceLocator, DIServiceLocatorAdapter>();
        }

        protected override void RegisterFrameworkExceptionTypes()
        {
            base.RegisterFrameworkExceptionTypes();
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(InvalidProgramException));
        }
    }
}

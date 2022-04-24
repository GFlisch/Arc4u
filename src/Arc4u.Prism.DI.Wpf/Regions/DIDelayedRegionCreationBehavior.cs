using Arc4u.Dependency.Attribute;
using Prism.Regions;
using Prism.Regions.Behaviors;

namespace Prism.DI.Regions
{
    [Export(typeof(DelayedRegionCreationBehavior))]
    public class DIDelayedRegionCreationBehavior : DelayedRegionCreationBehavior
    {
        public DIDelayedRegionCreationBehavior(RegionAdapterMappings regionAdapterMappings) : base(regionAdapterMappings)
        {
        }
    }
}

using Prism.Regions;
using Prism.Regions.Behaviors;
using System.Composition;

namespace Prism.DI.Regions
{
    [Export(typeof(DelayedRegionCreationBehavior))]
    public class DIDelayedRegionCreationBehavior : DelayedRegionCreationBehavior
    {
        [ImportingConstructor]
        public DIDelayedRegionCreationBehavior(RegionAdapterMappings regionAdapterMappings) : base(regionAdapterMappings)
        {
        }
    }
}

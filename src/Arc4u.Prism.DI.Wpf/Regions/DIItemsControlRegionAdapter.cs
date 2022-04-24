using Arc4u.Dependency.Attribute;
using Prism.Regions;

namespace Prism.DI.Regions
{
    [Export(typeof(DIItemsControlRegionAdapter))]
    public class DIItemsControlRegionAdapter : ItemsControlRegionAdapter
    {
        public DIItemsControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }
    }
}

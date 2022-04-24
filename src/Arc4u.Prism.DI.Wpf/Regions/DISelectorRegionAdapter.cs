using Arc4u.Dependency.Attribute;
using Prism.Regions;

namespace Prism.DI.Regions
{
    [Export(typeof(DISelectorRegionAdapter))]
    public class DISelectorRegionAdapter : SelectorRegionAdapter
    {
        public DISelectorRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }
    }
}

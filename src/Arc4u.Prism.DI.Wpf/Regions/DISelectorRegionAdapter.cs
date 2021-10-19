using Prism.Regions;
using System.Composition;

namespace Prism.DI.Regions
{
    [Export(typeof(DISelectorRegionAdapter))]
    public class DISelectorRegionAdapter : SelectorRegionAdapter
    {
        [ImportingConstructor]
        public DISelectorRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }
    }
}

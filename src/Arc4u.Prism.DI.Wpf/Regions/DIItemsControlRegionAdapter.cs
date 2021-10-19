using Prism.Regions;
using System.Composition;

namespace Prism.DI.Regions
{
    [Export(typeof(DIItemsControlRegionAdapter))]
    public class DIItemsControlRegionAdapter : ItemsControlRegionAdapter
    {
        [ImportingConstructor]
        public DIItemsControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }
    }
}

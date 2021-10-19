using Prism.Regions;
using System.Composition;

namespace Prism.DI.Regions
{
    [Export(typeof(DIContentControlRegionAdapter))]
    public class DIContentControlRegionAdapter : ContentControlRegionAdapter
    {
        [ImportingConstructor]
        public DIContentControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }
    }
}

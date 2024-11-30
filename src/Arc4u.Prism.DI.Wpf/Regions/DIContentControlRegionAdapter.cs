using Arc4u.Dependency.Attribute;
using Prism.Regions;

namespace Prism.DI.Regions;

[Export(typeof(DIContentControlRegionAdapter))]
public class DIContentControlRegionAdapter : ContentControlRegionAdapter
{
    public DIContentControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
    {
    }
}

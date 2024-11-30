using Arc4u.Dependency.Attribute;
using Prism.Regions.Behaviors;

namespace Prism.DI.Regions;

[Export(typeof(BindRegionContextToDependencyObjectBehavior))]
public class DIBindRegionContextToDependencyObjectBehavior : BindRegionContextToDependencyObjectBehavior
{
    public DIBindRegionContextToDependencyObjectBehavior()
    {
    }
}

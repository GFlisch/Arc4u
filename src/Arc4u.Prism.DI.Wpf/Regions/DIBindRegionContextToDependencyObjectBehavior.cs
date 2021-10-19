using Prism.Regions.Behaviors;
using System.Composition;

namespace Prism.DI.Regions
{
    [Export(typeof(BindRegionContextToDependencyObjectBehavior))]
    public class DIBindRegionContextToDependencyObjectBehavior : BindRegionContextToDependencyObjectBehavior
    {
        [ImportingConstructor]
        public DIBindRegionContextToDependencyObjectBehavior()
        {
        }
    }
}

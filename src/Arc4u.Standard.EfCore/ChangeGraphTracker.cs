using Arc4u.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Arc4u.EfCore
{
    public static class ChangeGraphTracker
    {
        /// <summary>
        /// To be used with EfCore ChangeTracker.TrackGraph.
        /// Convert the PersistChange of the PersistEntity to the EfCore state.
        /// </summary>
        /// <param name="node"><see cref="EntityEntryGraphNode"/></param>
        public static void Tracker(EntityEntryGraphNode node)
        {
            node.Entry.State = node.Entry.Entity is IPersistEntity persistEntity ? persistEntity.PersistChange.Convert() : EntityState.Unchanged;

            //if (newState != node.Entry.State 
            //    && (node.Entry.State != EntityState.Detached || newState != EntityState.Unchanged))
            //    node.Entry.State = newState;
        }

    }
}

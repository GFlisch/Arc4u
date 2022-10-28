# Arc4u.Standard.EfCore


Two new classes are added in this .Net 5.0 package.
- ChangeGraphTracker
- PersisteChangeExtension

Linked to the new Guidance, this version of the framework integrates in a better way the Insert, Update or delete actions.

Each persisted entities are inheriting from Data.Entity<T> where most of the time T is a Guid. This entity contains PersistChange
enum defining what we do with an object:
- Delete
- Update
- Insert
- Unchanged.

The purpose of the ChangeGraphTracker is to parse the entities and set properly (via the PersistChangeExtension) the EntityState.

The Ef code to manipulate the entities becomes very simple.


```csharp

        public async Task SaveAsync(Domain entity, CancellationToken cancellationToken)
        {
            _databaseContext.ChangeTracker.TrackGraph(entity, e => ChangeGraphTracker.Tracker<Guid>(e));

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveAsync(ICollection<Domain> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
                _databaseContext.ChangeTracker.TrackGraph(entity, e => ChangeGraphTracker.Tracker<Guid>(e));

            await _databaseContext.SaveChangesAsync(cancellationToken);
        }

```

As you see, the state is set correctly now with one line of code at the datalayer level.



using System.Diagnostics;

namespace Arc4u.Data;

internal sealed class CollectionDebugView<T>
{
    private readonly ICollection<T> collection;

    public CollectionDebugView(ICollection<T> collection)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(collection);
#else
        if (collection == null)
        {
            throw new ArgumentNullException("collection");
        }
#endif
        this.collection = collection;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
        get
        {
            var array = new T[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }
    }
}

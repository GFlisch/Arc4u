using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Arc4u.Data
{
    internal sealed class CollectionDebugView<T>
    {
        private ICollection<T> collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }
}

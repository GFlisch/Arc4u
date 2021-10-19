using System.Collections.Generic;
using System.Threading;

namespace Arc4u.Serializer.ProtoBuf
{
    /// <summary>
    /// Utilities for lock-free updating
    /// </summary>
    static class InterlockedExtensions
    {
        /// <summary>
        /// Update a dictionary <paramref name="dictionary"/> with a new <paramref name="value"/> for <paramref name="key"/> in a thread-safe and lock-free way.
        /// If the key already exists, the value is replaced.
        /// This is ideal for dictionary that are infrequently updated like during serialization for associating types with other values (or vice-versa),
        /// but the algorithm is completely generic.
        /// </summary>
        /// <typeparam name="TKey">the type of the key</typeparam>
        /// <typeparam name="TValue">the type of the value</typeparam>
        /// <param name="dictionary">the dictionary to update</param>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        public static void InterlockedUpdate<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            /// This may be called from different threads. Since over time the number of types will stabilize, we use copy semantics to update it without locks.
            var current = dictionary;
            var copy = new Dictionary<TKey, TValue>(current);
            /// it might be that the type was already added via a previous call. We update it here (even though the information should be the same) using 
            /// the accessor instead of  <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
            copy[key] = value;

            if (Interlocked.CompareExchange(ref dictionary, copy, current) != current)
            {
                var spinner = new SpinWait();

                do
                {
                    spinner.SpinOnce();
                    current = dictionary;
                    copy = new Dictionary<TKey, TValue>(current);
                    copy[key] = value;
                }
                while (Interlocked.CompareExchange(ref dictionary, copy, current) != current);
            }
        }
    }
}

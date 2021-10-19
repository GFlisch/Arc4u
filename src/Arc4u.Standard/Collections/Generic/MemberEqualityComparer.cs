using System;
using System.Collections.Generic;

namespace Arc4u.Collections.Generic
{
    /// <summary>
    /// Defines methods to support the comparison of objects for equality 
    /// based on specified member(s) of those objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    public sealed class MemberEqualityComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, object> _selector;

        MemberEqualityComparer(Func<T, object> selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");
            _selector = selector;
        }

        /// <summary>
        /// Returns a default equality comparer based on the member(s) specified by the <paramref name="selector"/>
        /// for the type specified by the generic argument.
        /// </summary>
        /// <param name="selector">The selector of the member(s) used for comparaison.</param>
        /// <returns>The default instance of the <see cref="MemberEqualityComparer&lt;T&gt;"/> class for type <typeparamref name="T"/>.</returns>
        public static MemberEqualityComparer<T> Default(Func<T, object> selector)
        {
            return new MemberEqualityComparer<T>(selector);
        }

        /// <summary>
        /// Determines whether two objects of type <typeparamref name="T"/> are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y)
                || (x != null && y != null && Equals(_selector(x), _selector(y)));
        }

        /// <summary>
        /// Serves as a hash function for the specified object for hashing algorithms and data structures, such as a hash table.
        /// </summary>
        /// <param name="obj">The object for which to get a hash code.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return (obj == null || _selector(obj) == null)
                ? 0
                : _selector(obj).GetHashCode();
        }
    }
}

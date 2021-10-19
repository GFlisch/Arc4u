using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Arc4u
{
    /// <summary>
    /// Represents a collection of intervals. This class cannot be inherited. 
    /// </summary>
    /// <typeparam name="T">The value type of the interval.</typeparam>
    public sealed class IntervalCollection<T> : ReadOnlyCollection<Interval<T>>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IntervalCollection`1"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="args"/> is null.</exception>
        public IntervalCollection(params Interval<T>[] args)
            : base(args)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IntervalCollection`1"/> class.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the <see cref="T:IntervalCollection`1"/>.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public IntervalCollection(IList<Interval<T>> collection)
            : base(collection)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the intersection of the current <see cref="T:IntervalCollection`1"/>.
        /// </summary>
        /// <value>The intersection.</value>
        public Interval<T> Intersection
        {
            get { return Interval.IntersectionOf(this); }
        }

        /// <summary>
        /// Gets the complement of the current <see cref="T:IntervalCollection`1"/>.
        /// </summary>
        /// <value>The complement.</value>
        public IntervalCollection<T> Complement
        {
            get { return Interval.ComplementOf(this); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance represents the default empty set.
        /// </summary>
        /// <value><c>true</c> if this instance contains no element; otherwise, <c>false</c>.</value>
        /// <seealso href="http://en.wikipedia.org/wiki/Empty_set">Empty Set</seealso>
        public bool IsEmpty
        {
            get
            {
                return (from item in base.Items
                        where !item.IsEmpty
                        select item).FirstOrDefault() == default(Interval<T>);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance represents an empty set of the specified element.
        /// </summary>
        /// <param name="value">The element value.</param>
        /// <returns><c>true</c> if this instance contains no element; otherwise, <c>false</c>.</returns>
        /// <seealso cref="IsEmpty"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Empty_set">Empty Set</seealso>
        internal bool IsEmptyOf(T value)
        {
            return (from item in base.Items
                    where !item.IsEmptyOf(value)
                    select item).FirstOrDefault() == default(Interval<T>);
        }


        /// <summary>
        /// Gets a value indicating whether this instance represents an universe.
        /// </summary>
        /// <value><c>true</c> if this instance represents an universe; otherwise, <c>false</c>.</value>
        /// <seealso href="http://en.wikipedia.org/wiki/Universe_(mathematics)">Universe</seealso>
        public bool IsUniverse
        {
            get
            {
                return this.Count != 0
                    && (from item in base.Items
                        where !item.IsUniverse
                        select item).FirstOrDefault() == default(Interval<T>);
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance represents a singleton.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance contains only one element; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="IsSingletonOf"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Singleton_(mathematics)">Singleton</seealso>
        public bool IsSingleton
        {
            get
            {
                return this.Count != 0
                    && (from item in base.Items
                        where !item.IsSingleton
                        select item).FirstOrDefault() == default(Interval<T>);
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance represents a singleton of the specified element.
        /// </summary>
        /// <param name="value">The element value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance contains only the specified <paramref name="value"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="IsSingleton"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Singleton_(mathematics)">Singleton</seealso>
        public bool IsSingletonOf(T value)
        {
            return this.Count != 0
                && (from item in base.Items
                    where !item.IsSingletonOf(value)
                    select item).FirstOrDefault() == default(Interval<T>);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the union of the current <see cref="T:IntervalCollection`1"/>.
        /// </summary>
        /// <value>The union.</value>
        public IntervalCollection<T> Union()
        {
            return Union(UnionDenominator.Highest);
        }


        /// <summary>
        /// Gets the union of the current <see cref="T:IntervalCollection`1"/> according to the specified <paramref name="denominator"/>.
        /// </summary>
        /// <param name="denominator">The denominator considered while performing the union.</param>
        /// <value>The union.</value>
        /// <remarks>
        /// While performing an <see cref="Interval.UnionOf&lt;T&gt;">UnionOf</see> intervals, depending of the specified <see cref="UnionDenominator"/>, the result will be different.
        /// If you consider the following intervals:
        /// <list type="table">
        /// <item>
        /// <term>a</term>
        /// <description>]∞ ; 3[</description>
        /// </item>
        /// <item>
        /// <term>b</term>
        /// <description>]0 ; 5[</description>
        /// </item>
        /// <item>
        /// <term>c</term>
        /// <description>[1 ; 2]</description>
        /// </item>
        /// </list>
        /// The union with the <see cref="UnionDenominator.Highest"/> denominator will return an <see cref="IntervalCollection&lt;T&gt;"/> of 1 element as shown below:
        /// <list type="table">
        /// <item>
        /// <term>i</term>
        /// <description>]∞ ; 5[</description>
        /// </item>
        /// </list>
        /// The union with the <see cref="UnionDenominator.Lowest"/> denominator will return an <see cref="IntervalCollection&lt;T&gt;"/> of 5 element as shown below:
        /// <list type="table">
        /// <item>
        /// <term>i</term>
        /// <description>]∞ ; 0]</description>
        /// </item>
        /// <item>
        /// <term>j</term>
        /// <description>]0 ; 1[</description>
        /// </item>
        /// <item>
        /// <term>k</term>
        /// <description>[1 ; 2]</description>
        /// </item>
        /// <item>
        /// <term>l</term>
        /// <description>]2 ; 3[</description>
        /// </item>
        /// <item>
        /// <term>m</term>
        /// <description>[3 ; 5[</description>
        /// </item>
        /// </list>
        /// </remarks>
        public IntervalCollection<T> Union(UnionDenominator denominator)
        {
            return (denominator == UnionDenominator.Highest)
                ? Interval.HighUnionOf(this)
                : Interval.LowUnionOf(this);
        }

        #endregion
    }
}
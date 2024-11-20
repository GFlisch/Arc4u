using System.Collections;
using System.Reflection;

namespace System.Linq;

public class Switch<TSource, TResult> : IEnumerable<TResult>
{
    #region nested classes
    private class CaseSelector<TSelectorSource, TSelectorResult>
    {
        private readonly Func<TSource, bool> predicate;
        private readonly Func<TSource, TResult> selector;

        public CaseSelector(Func<TSource, bool> predicate, Func<TSource, TResult> selector)
        {
            predicate = predicate;
            selector = selector;
        }

        public bool CanSelect(TSource source) { return predicate(source); }

        public TResult Select(TSource source) { return selector(source); }
    }

    private class CaseSelector<TSelectorSource, TCase, TSelectorResult> : CaseSelector<TSelectorSource, TSelectorResult>
    {
        public CaseSelector(Func<TCase, TResult> selector) : base(
            x => x is TCase,
            x => x is TCase tCase ? selector(tCase) : throw new InvalidOperationException("Invalid case type")
        )
        { }
    }
    #endregion

    private readonly IEnumerable<TSource> source;
    private readonly IList<CaseSelector<TSource, TResult>> casePredicates = new List<CaseSelector<TSource, TResult>>();

    public Switch(IEnumerable<TSource> source)
    {
        source = source;
    }

    public Switch<TSource, TResult> Case(Func<TSource, bool> predicate, Func<TSource, TResult> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
#else
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif
        casePredicates.Add(new CaseSelector<TSource, TResult>(predicate, selector));

        return this;
    }

    public Switch<TSource, TResult> Case(Type type, Func<TSource, TResult> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
#else
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif

        casePredicates.Add(new CaseSelector<TSource, TResult>(
            x => type?.GetTypeInfo().IsAssignableFrom(x?.GetType().GetTypeInfo()) ?? false,
            selector
        ));
        return this;
    }

    public Switch<TSource, TResult> Case<TCase>(Func<TCase, TResult> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
#else
        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif
        casePredicates.Add(new CaseSelector<TSource, TCase, TResult>(selector));

        return this;
    }

    #region IEnumerable<TResult> Members
    public IEnumerator<TResult> GetEnumerator()
    {
        foreach (var item in source)
        {
            var switchCase = casePredicates.FirstOrDefault(x => x.CanSelect(item));

            if (switchCase != null)
            {
                yield return switchCase.Select(item);
            }
        }
    }
    #endregion

    #region IEnumerable Members
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}

public static class SwitchExtensions
{
    public static Switch<TSource, TResult> Case<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        Func<TSource, TResult> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
#else
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif
        return new Switch<TSource, TResult>(source).Case(predicate, selector);
    }

    public static Switch<TSource, TResult> Case<TSource, TResult>(this IEnumerable<TSource> source,
        Type type,
        Func<TSource, TResult> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(type, nameof(type));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
#else
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif
        return new Switch<TSource, TResult>(source).Case(type, selector);
    }

    public static Switch<TSource, TResult> AsSwitch<TSource, TResult>(this IEnumerable<TSource> source)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
#else
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
#endif
        return new Switch<TSource, TResult>(source);
    }
}

/// <summary>
/// Provides a set of static methods extending the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> class.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Filters recursively a sequence of values based on a predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to filter recursively.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="selector">A recursive function to apply to each element.</param>
    /// <returns>An <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> that contains elements from the input sequence that satisfy the condition.</returns>
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source
        , Func<TSource, bool> predicate
        , Func<TSource, IEnumerable<TSource>> selector)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(selector);
#else
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }
#endif
        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }

            var innerSource = selector(item);
            if (innerSource != null)
            {
                foreach (var innerItem in Where(innerSource, predicate, selector))
                {
                    if (predicate(innerItem))
                    {
                        yield return innerItem;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Take a collection of an object of type TSource and use the getChildrenFunction to travel
    /// the structure of the collection. In return, we have a flatten collection of each nodes.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to flatten recursively.</param>
    /// <param name="getChildrenFunction">Return the property to use to travel through the node collection.</param>
    /// <returns></returns>
    public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> getChildrenFunction)
    {
        if (null == source)
        {
            return new List<TSource>();
        }
        // Add what we have to the stack
        var flattenedList = source;

        // Go through the input enumerable looking for children,
        // and add those if we have them
        foreach (TSource element in source)
        {
            flattenedList = flattenedList.Concat(
              getChildrenFunction(element).Flatten(getChildrenFunction)
            );
        }
        return flattenedList;
    }
}

namespace System.Collections.Generic;

/// <summary>
/// Provides a set of static methods extending the <see cref="System.Collections.Generic.List&lt;T&gt;"/> class.
/// </summary>
public static class ListExtensions
{
    /// <summary>Substracts all the items from the <see cref="System.Collections.Generic.List&lt;T&gt;"/> that match the conditions defined by the specified predicate.</summary>
    /// <returns>The substracted items from the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.</returns>
    /// <param name="col">The collection from where items are substracted.</param>
    /// <param name="match">The <see cref="T:System.Predicate`1"></see> delegate that defines the conditions of the items to substract.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="col"/> is null or <paramref name="match"/> is null.</exception>        
    public static List<T> Substract<T>(this List<T> col, Predicate<T> match)
    {
        var result = new List<T>();
        var index = 0;
        while ((index < col.Count) && !match(col[index]))
        {
            index++;
        }
        if (index >= col.Count)
        {
            return result;
        }

        result.Add(col[index]);
        var num2 = index + 1;
        while (num2 < col.Count)
        {
            while ((num2 < col.Count) && match(col[num2]))
            {
                result.Add(col[num2]);
                num2++;
            }
            if (num2 < col.Count)
            {
                col[index++] = col[num2++];
            }
        }
        col.RemoveRange(index, col.Count - index);

        return result;
    }

    /// <summary>Substracts the first items from the <see cref="System.Collections.Generic.List&lt;T&gt;"/> that match the conditions defined by the specified predicate.</summary>
    /// <returns>The first substracted items from the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.</returns>
    /// <param name="col">The collection from where items are substracted.</param>
    /// <param name="match">The <see cref="T:System.Predicate`1"></see> delegate that defines the conditions of the items to substract.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="col"/> is null or <paramref name="match"/> is null.</exception>        
    /// <remarks>Using this methods makes sense if the <paramref name="col"/> is sorted according to the specified <paramref name="match"/>.</remarks>
    public static List<T> SubstractFirst<T>(this List<T> col, Predicate<T> match)
    {
        var result = new List<T>();
        var index = 0;
        while ((index < col.Count) && !match(col[index]))
        {
            index++;
        }
        if (index >= col.Count)
        {
            return result;
        }

        result.Add(col[index]);
        var num2 = index + 1;
        var firstPassed = false;
        while (num2 < col.Count)
        {
            while (!firstPassed && (num2 < col.Count) && match(col[num2]))
            {
                result.Add(col[num2]);
                num2++;
            }
            if (num2 < col.Count)
            {
                firstPassed = true;
                col[index++] = col[num2++];
            }
        }

        col.RemoveRange(index, col.Count - index);

        return result;
    }

    /// <summary>Substracts the first item from the <see cref="System.Collections.Generic.List&lt;T&gt;"/> that match the conditions defined by the specified predicate.</summary>
    /// <param name="col">The collection from where item is substracted.</param>
    /// <returns>The first substracted item from the <see cref="System.Collections.Generic.List&lt;T&gt;"/>.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"></see> delegate that defines the conditions of the item to substract.</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="col"/> is null or <paramref name="match"/> is null.</exception>        
    public static T? SubstractFirstOne<T>(this List<T> col, Predicate<T> match)
    {
        var result = default(T);
        var index = -1;

        for (var i = 0; i < col.Count; i++)
        {
            if (match(col[i]))
            {
                result = col[i];
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            col.RemoveAt(index);
        }

        return result;
    }
}

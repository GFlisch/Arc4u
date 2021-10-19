namespace System.Collections.Generic
{
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
            if (col == null)
                throw new ArgumentNullException("col");
            if (match == null)
                throw new ArgumentNullException("match");

            var result = new List<T>();
            int index = 0;
            while ((index < col.Count) && !match(col[index]))
            {
                index++;
            }
            if (index >= col.Count)
            {
                return result;
            }

            result.Add(col[index]);
            int num2 = index + 1;
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
            if (col == null)
                throw new ArgumentNullException("col");
            if (match == null)
                throw new ArgumentNullException("match");

            var result = new List<T>();
            int index = 0;
            while ((index < col.Count) && !match(col[index]))
            {
                index++;
            }
            if (index >= col.Count)
            {
                return result;
            }

            result.Add(col[index]);
            int num2 = index + 1;
            bool firstPassed = false;
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
        public static T SubstractFirstOne<T>(this List<T> col, Predicate<T> match)
        {
            if (col == null)
                throw new ArgumentNullException("col");
            if (match == null)
                throw new ArgumentNullException("match");

            var result = default(T);
            var index = -1;

            for (int i = 0; i < col.Count; i++)
            {
                if (match(col[i]))
                {
                    result = col[i];
                    index = i;
                    break;
                }
            }

            if (index >= 0)
                col.RemoveAt(index);

            return result;
        }

#if SILVERLIGHT
        public static int RemoveAll<T>(this List<T> col, Predicate<T> match)
        {
            if (col == null)
                throw new ArgumentNullException("col");
            if (match == null)
                throw new ArgumentNullException("match");

            int index = 0;
            while ((index < col.Count) && !match(col[index]))
            {
                index++;
            }
            if (index >= col.Count)
            {
                return 0;
            }
            int num2 = index + 1;
            while (num2 < col.Count)
            {
                while ((num2 < col.Count) && match(col[num2]))
                {
                    num2++;
                }
                if (num2 < col.Count)
                {
                    col[index++] = col[num2++];
                }
            }

            int num3 = col.Count - index;
            col.RemoveRange(index, num3);
            return num3;
        }

        public static bool Exists<T>(this List<T> col, Predicate<T> match)
        {
            return (FindIndex(col, match) != -1);
        }

        public static int FindIndex<T>(this List<T> col, Predicate<T> match)
        {
            return FindIndex(col, 0, col.Count, match);
        }

        public static int FindIndex<T>(this List<T> col, int startIndex, int count, Predicate<T> match)
        {
            if (col == null)
                throw new ArgumentNullException("col");
            if (startIndex > col.Count)
                throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");                
            if ((count < 0) || (startIndex > (col.Count - count)))
                throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");                
            if (match == null)
                throw new ArgumentNullException("match");

            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(col[i]))
                {
                    return i;
                }
            }
            return -1;
        }
#endif
    }
}

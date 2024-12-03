namespace Arc4u.Collections.Generic;

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
        ArgumentNullException.ThrowIfNull(selector);

        _selector = selector;
    }

    public static MemberEqualityComparer<T> Default(Func<T, object> selector)
    {
        return new MemberEqualityComparer<T>(selector);
    }

    public bool Equals(T? x, T? y)
    {
        return ReferenceEquals(x, y)
            || (x != null && y != null && Equals(_selector(x), _selector(y)));
    }

    public int GetHashCode(T? obj)
    {
        return (obj == null || _selector(obj) == null)
            ? 0
            : _selector(obj).GetHashCode();
    }
}

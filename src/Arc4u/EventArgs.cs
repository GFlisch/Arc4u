namespace Arc4u;

/// <summary>
/// Define a generic event argument.
/// </summary>
/// <typeparam name="T">The generic type used in the event argument.</typeparam>
/// <remarks>
/// Create a new Event argument with the value assigned.
/// </remarks>
/// <param name="value"></param>
public class EventArgs<T>(T value) : EventArgs
{

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T Value
    {
        get { return value; }
    }
}

/// <summary>
/// Define a generic event argument with both values of type T1 and T2.
/// </summary>
/// <typeparam name="T1">The generic type used in the first event argument.</typeparam>
/// /// <typeparam name="T2">The generic type used in the second event argument.</typeparam>
/// <remarks>
/// Create a new Event argument with the value assigned.
/// </remarks>
/// <param name="value1">The first value of type T1.</param>
/// <param name="value2">The first value of type T2.</param>
public class EventArgs<T1, T2>(T1 value1, T2 value2) : EventArgs
{

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T1 Value
    {
        get { return value1; }
    }

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T2 Value2
    {
        get { return value2; }
    }
}

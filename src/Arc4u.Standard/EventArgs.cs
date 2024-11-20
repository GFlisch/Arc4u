namespace Arc4u;

/// <summary>
/// Define a generic event argument.
/// </summary>
/// <typeparam name="T">The generic type used in the event argument.</typeparam>
public class EventArgs<T> : EventArgs
{
    /// <summary>
    /// Create a new Event argument with the value assigned.
    /// </summary>
    /// <param name="value"></param>
    public EventArgs(T value)
    {
        m_value = value;
    }

    private readonly T m_value;

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T Value
    {
        get { return m_value; }
    }
}

/// <summary>
/// Define a generic event argument with both values of type T1 and T2.
/// </summary>
/// <typeparam name="T1">The generic type used in the first event argument.</typeparam>
/// /// <typeparam name="T2">The generic type used in the second event argument.</typeparam>
public class EventArgs<T1, T2> : EventArgs
{
    /// <summary>
    /// Create a new Event argument with the value assigned.
    /// </summary>
    /// <param name="value1">The first value of type T1.</param>
    /// <param name="value2">The first value of type T2.</param>
    public EventArgs(T1 value1, T2 value2)
    {
        m_value = value1;
        m_value2 = value2;
    }

    private readonly T1 m_value;

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T1 Value
    {
        get { return m_value; }
    }

    private readonly T2 m_value2;

    /// <summary>
    /// Get the value assigned during construction of the event argument.
    /// </summary>
    public T2 Value2
    {
        get { return m_value2; }
    }
}

using System.Globalization;

namespace Arc4u.Threading;

/// <summary>
/// Scopes a type instance.
/// </summary>
/// <typeparam name="T">Any type.</typeparam>
public class Scope<T> : IDisposable
{
    private static readonly AsyncLocal<Scope<T>?> _instance = new AsyncLocal<Scope<T>?>();

    private bool Disposed { get; set; }
    private bool ToDispose { get; set; }
    private Scope<T>? Parent { get; set; }

    private T Value;

    /// <summary>
    /// Prevents a default instance of the <see cref="Scope&lt;T&gt;"/> class from being created.
    /// </summary>
    private Scope()
    {
        Value = default!;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Scope&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="instance">The instance.</param>
    public Scope(T instance)
        : this(instance, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Scope&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="toDispose">Indicates if the scoped <paramref name="instance"/> is disposed when its scope is disposed.</param>
    public Scope(T instance, bool toDispose)
    {
        Value = instance;
        Parent = _instance.Value;
        _instance.Value = this;
        ToDispose = toDispose;
    }

    /// <summary>
    /// Gets the current instance of T in the ambient scope.
    /// </summary>
    /// <value>The current instance of T.</value>
    public static T? Current
    {
        get => _instance.Value == null ? default : _instance.Value.Value;

        set
        {
            if (_instance.Value != null && null != value)
            {
                _instance.Value.Value = value;
            }
        }
    }

    protected static Scope<T>? Ambient
    {
        get { return _instance.Value; }
    }

    protected T? ParentValue
    {
        get { return null == Parent ? default(T) : Parent.Value; }
    }

    /// <summary>
    /// Disposes the ambient scope and its current instance when applicable.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the ambient scope and its current instance when applicable.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                if (ToDispose)
                {
                    var disposable = Current as IDisposable;
                    disposable?.Dispose();
                }
            }

            _instance.Value = Parent;
            Disposed = true;
        }
    }

    /// <summary>
    /// Returns a <see cref="System.string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.string"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return Current != null
            ? string.Format(CultureInfo.InvariantCulture, "Scoping: {0}", Current)
            : base.ToString()!;
    }
}

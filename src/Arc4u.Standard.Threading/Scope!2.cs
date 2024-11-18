using System.Globalization;

namespace Arc4u.Threading;

/// <summary>
/// Scopes a type instance.
/// </summary>
/// <typeparam name="TScope">The type of the scope.</typeparam>
/// <typeparam name="TInstance">The type of the instance.</typeparam>
public class Scope<TScope, TInstance> : IDisposable
{
    private static readonly AsyncLocal<Scope<TScope, TInstance>?> _instance = new AsyncLocal<Scope<TScope, TInstance>?>();

    private bool Disposed { get; set; }
    private bool ToDispose { get; set; }
    private Scope<TScope, TInstance>? Parent { get; set; }

    private TInstance Value;

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
    public Scope(TInstance instance)
        : this(instance, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Scope&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="dispose">Indicates if the scoped <paramref name="instance"/> is disposed when its scope is disposed.</param>
    public Scope(TInstance instance, bool toDispose)
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
    public static TInstance? Current
    {
        get
        {
            return null == _instance.Value ? default(TInstance) : _instance.Value.Value;
        }
        set
        {
            if (_instance.Value != null && null != value)
            {
                _instance.Value.Value = value;
            }
        }
    }

    protected TInstance? ParentValue
    {
        get { return null == Parent ? default(TInstance) : Parent.Value; }
    }

    protected static Scope<TScope, TInstance>? Ambient
    {
        get { return _instance.Value; }
    }

    /// <summary>
    /// Disposes the ambient scope and its current instance when applicable.
    /// </summary>
    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;

            if (ToDispose)
            {
                var disposable = Current as IDisposable;
                disposable?.Dispose();
            }
        }

        _instance.Value = Parent;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return Current != null
            ? string.Format(CultureInfo.InvariantCulture, "Scoping: {0}", Current)
            : base.ToString()!;
    }
}

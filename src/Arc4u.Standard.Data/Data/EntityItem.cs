using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Arc4u.Data;

/// <summary>
/// Represents an entity whose the relationship with its parent 
/// requires a self-tracked <see cref="PersistChange"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>

[DataContract(Name = "EntityItemOf{0}")]
public sealed class EntityItem<TEntity> : PersistEntity
{
    private const string InvalidPersistChange = "Invalid PersistChange {0}.";
    private const string InconsistentPersistChange = "Inconsistent PersistChange between the EntityItem {0} and its Entity {1}.";
    private PersistEntity? _persistEntity;
    private TEntity? _entity;

    /// <summary>
    /// Gets the entity.
    /// </summary>
    /// <value>The entity.</value>
    [DataMember(EmitDefaultValue = false)]
    public TEntity? Entity
    {
        get { return _entity; }
        set
        {
            _entity = value;
            if (_persistEntity != null)
            {
                _persistEntity.PropertyChanged -= Entity_PropertyChanged;
            }

            _persistEntity = value as PersistEntity;
            if (_persistEntity != null)
            {
                _persistEntity.PropertyChanged += new PropertyChangedEventHandler(Entity_PropertyChanged);
            }
        }
    }

    /// <summary>
    /// Gets or sets the persist change.
    /// </summary>
    /// <value>The persist change.</value>
    /// <exception cref="ArgumentException">The persist change is not allowed.</exception>
    [DataMember(EmitDefaultValue = false)]
    public override PersistChange PersistChange
    {
        get { return _persistChange; }
        set
        {
            if (_persistChange != value)
            {
                // consider invalid PersistChange
                if (value == PersistChange.Update)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, InvalidPersistChange, value));
                }

                // prevent invalid transitions except when serializing / deserializing
                if (!IgnoreOnPropertyChanged
                    && (_persistChange == PersistChange.Insert && value == PersistChange.Delete
                    || _persistChange == PersistChange.Delete && value == PersistChange.Insert))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, InvalidTransition, _persistChange, value));
                }

                _persistChange = value;
                RaisePropertyChanged(PersistChangePropertyName);
            }
        }
    }

    #region Constructors

    private EntityItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityItem&lt;TEntity&gt;"/> class 
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
    public EntityItem(TEntity entity) : this(entity, PersistChange.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityItem&lt;TEntity&gt;"/> class
    /// with the specified <see cref="PersistChange"/>.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistChange">The persist change.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
    public EntityItem(TEntity entity, PersistChange persistChange)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _entity = entity;
        _persistEntity = entity as PersistEntity;

        if (_persistEntity != null)
        {
            _persistEntity.PropertyChanged += new PropertyChangedEventHandler(Entity_PropertyChanged);
        }

        _persistChange = persistChange;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityItem&lt;TEntity&gt;"/> class
    /// copied from the specified <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
    public EntityItem(EntityItem<TEntity> item)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#else
        if (item == null)
        {
            throw new ArgumentNullException("item");
        }
#endif
        _entity = item._entity;
        _persistChange = item._persistChange;
        _persistEntity = item._persistEntity;
    }

    #endregion

    private void Entity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //consider only necessary cases
        if (!string.Equals(e.PropertyName, PersistChangePropertyName))
        {
            return;
        }

        //consider PersistChange inconsistency
        if ((PersistChange == PersistChange.Insert && _persistEntity?.PersistChange == PersistChange.Delete) ||
            (PersistChange == PersistChange.None && _persistEntity?.PersistChange == PersistChange.Delete))
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, InconsistentPersistChange, PersistChange, _persistEntity.PersistChange));
        }
    }

    #region Overriden Members

    /// <summary>
    /// Converts the value of the current <see cref="EntityItem&lt;TEntity&gt;"/> object to its equivalent string representation. 
    /// </summary>
    /// <returns>A string representation of the value of the current <see cref="EntityItem&lt;TEntity&gt;"/> object.</returns>        
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "PersistChange = {0}, Entity = {1}",
            PersistChange,
            (Entity != null)
                ? Entity.ToString()
                : string.Empty);
    }

    #endregion

    #region Implicit / Explicit Operators

    /// <summary>
    /// Performs an implicit conversion from TEntity to <see cref="EntityItem&lt;TEntity&gt;"/>.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator EntityItem<TEntity>(TEntity entity)
    {
        return new EntityItem<TEntity>(entity);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="EntityItem&lt;TEntity&gt;"/> to TEntity.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator TEntity?(EntityItem<TEntity>? item)
    {
        return item != null ? item.Entity : default(TEntity);
    }

    #endregion
}

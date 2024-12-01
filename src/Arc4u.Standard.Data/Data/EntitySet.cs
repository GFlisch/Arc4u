using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Arc4u.Data;

/// <summary>
/// <para>Represents a strongly typed set of entities supporting change tracking, change notification.</para>
/// <para>Change tracking support concerns entities removed from the set. 
/// an inserted entity removed from the set will be physically removed 
/// whereas any other entities will be marked as deleted.</para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>

[DebuggerTypeProxy(typeof(CollectionDebugView<>)), DebuggerDisplay("Count = {Count}")]
public sealed class EntitySet<TEntity>
    : INotifyCollectionChanged
    , INotifyPropertyChanged
    , IList<TEntity>
    , IList
    where TEntity : IPersistEntity
{
    #region string Constants
    const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
    const string ArgumentOutOfRange_Count = "Count must be positive and count must refer to a location within the string/array/collection.";
    const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
    const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of entities from index to the end of the source collection.";
    const string ArgumentOutOfRange_BiggerThanCollection = "Larger than collection size.";
    const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
    const string InvalidOperation_EnumOpCantHappen = "Enumeration has either not started or has already finished.";
    const string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";
    const string ArgumentOutOfRange_SmallCapacity = "capacity was less than the current size.";
    const string ArgumentOutOfRange_ListInsert = "Index must be within the bounds of the List.";
    const string Argument_InvalidArrayType = "Target array type is not compatible with the type of entities in the collection.";
    const string Arg_WrongType = "The value '{0}' is not of type '{1}' and cannot be used in this generic collection.";
    const string ObservableCollectionReentrancyNotAllowed = "Cannot change ObservableCollection during a CollectionChanged event.";
    #endregion

    private static readonly TEntity[] _emptyArray = [];
    private TEntity[] _items;
    private int _size;

    private object? _syncRoot;
    private int _version;
    private readonly SimpleMonitor _monitor;
    private const string ItemPropertyName = "Item[]";

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Occurs after the value of a property is changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="E:EntitySet&lt;TEntity&gt;.PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property whose value has changed.</param>
    void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="E:EntitySet&lt;TEntity&gt;.PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.ComponentModel.PropertyChangedEventArgs"/> that contains the event data.</param>
    void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        //keep handler to avoid race codition
        PropertyChanged?.Invoke(this, e);
    }

    #endregion

    #region INotifyCollectionChanged Members

    /// <summary>
    /// Occurs when an entity is added, removed, changed, moved, or the entire list is refreshed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    #endregion

    #region Observable Members

    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event with the provided arguments.
    /// </summary>
    /// <param name="e">Arguments of the event being raised.</param>
    /// <remarks>
    /// <para>Properties and methods that modify this collection raise the <see cref="CollectionChanged"/> event through this virtual method.</para>
    /// </remarks>
    void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (CollectionChanged != null)
        {
            using (BlockReentrancy())
            {
                CollectionChanged(this, e);
            }
        }
    }

    void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
    }

    private void OnCollectionReset()
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Disallows reentrant attempts to change this collection.
    /// </summary>
    /// <returns>An <see cref="T:System.Idisposable"/> object that can be used to dispose of the object.</returns>
    SimpleMonitor BlockReentrancy()
    {
        _monitor.Enter();
        return _monitor;
    }

    /// <summary>
    /// Checks for reentrant attempts to change this collection.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">If there was a call to <see cref="BlockReentrancy"/> of which the <see cref="T:System.Idisposable"/> return value has not yet been disposed of. Typically, this means when there are additional attempts to change this collection during a <see cref="CollectionChanged"/> event. However, it depends on when derived classes choose to call <see cref="BlockReentrancy"/>.</exception>
    void CheckReentrancy()
    {
        if ((_monitor.Busy && (CollectionChanged != null)) && (CollectionChanged.GetInvocationList().Length > 1))
        {
            throw new InvalidOperationException(ObservableCollectionReentrancyNotAllowed);
        }
    }

    /// <summary>
    /// Moves the entity at the specified index to a new location in the <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    /// <param name="oldIndex">The zero-based index specifying the location of the entity to be moved.</param>
    /// <param name="newIndex">The zero-based index specifying the new location of the entity.</param>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public void Move(int oldIndex, int newIndex)
    {
        CheckReentrancy();
        var entity = this[oldIndex];
        BaseRemoveAt(oldIndex);
        InsertItem(newIndex, entity);
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(NotifyCollectionChangedAction.Move, entity, newIndex, oldIndex);
    }

    #endregion

    /// <summary>
    /// Propagates the persist change to the entities of the current <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    /// <param name="persistChange">The persist change.</param>
    public void PropagatePersistChange(PersistChange persistChange)
    {
        foreach (var item in this)
        {
            if (item != null)
            {
                item.PersistChange = persistChange;
            }
        }
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySet&lt;TEntity&gt;"/> class.
    /// </summary>
    public EntitySet()
        : base()
    {
        _items = EntitySet<TEntity>._emptyArray;
        _monitor = new SimpleMonitor();
    }

    /// <summary>Initializes a new instance of the <see cref="EntitySet&lt;TEntity&gt;"/> class that is empty and has the specified initial capacity.</summary>
    /// <param name="capacity">The number of entities that the new set can initially store.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">capacity is less than 0. </exception>
    public EntitySet(int capacity)
        : base()
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), ArgumentOutOfRange_SmallCapacity);
        }
        _items = new TEntity[capacity];
        _monitor = new SimpleMonitor();
    }

    /// <summary>Initializes a new instance of the <see cref="EntitySet&lt;TEntity&gt;"/> class that contains entities copied from the specified collection and has sufficient capacity to accommodate the number of entities copied.</summary>
    /// <param name="collection">The collection whose entities are copied to the new set.</param>
    /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
    public EntitySet(IEnumerable<TEntity> collection)
        : base()
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(collection);
#else
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
#endif
        _monitor = new SimpleMonitor();

        if (collection is ICollection<TEntity> is2)
        {
            var count = is2.Count;
            _items = new TEntity[count];
            is2.CopyTo(_items, 0);
            _size = count;
        }
        else
        {
            _size = 0;
            _items = new TEntity[4];
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Add(enumerator.Current);
            }
        }
    }
    #endregion

    /// <summary>Adds an object to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="item">The object to be added to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public void Add(TEntity item)
    {
        CheckReentrancy();
        var index = _size;
        AddItem(item);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    /// <summary>Adds an object to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="entity">The object to be added to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    void AddItem(TEntity entity)
    {
        if (_size == _items.Length)
        {
            EnsureCapacity(_size + 1);
        }

        _items[_size++] = entity;
        _version++;
    }

    /// <summary>Adds the entities of the specified collection to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="collection">The collection whose entities should be added to the end of the <see cref="EntitySet&lt;TEntity&gt;"/>. The collection itself cannot be null, but it can contain entities that are null, if type TEntity is a reference type.</param>
    /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
    public void AddRange(IEnumerable<TEntity> collection)
    {
        InsertRange(_size, collection);
    }

    /// <summary>Returns a read-only <see cref="T:System.Collections.Generic.IList`1"/> wrapper for the current collection.</summary>
    /// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/> that acts as a read-only wrapper around the current <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    public ReadOnlyCollection<TEntity> AsReadOnly()
    {
        return new ReadOnlyCollection<TEntity>(this);
    }

    /// <summary>Searches the entire sorted <see cref="EntitySet&lt;TEntity&gt;"/> for an entity using the default comparer and returns the zero-based index of the entity.</summary>
    /// <returns>The zero-based index of entity in the sorted <see cref="EntitySet&lt;TEntity&gt;"/>, if entity is found; otherwise, a negative number that is the bitwise complement of the index of the next entity that is larger than entity or, if there is no larger entity, the bitwise complement of <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</returns>
    /// <param name="entity">The object to locate. The value can be null for reference types.</param>
    /// <exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    public int BinarySearch(TEntity entity)
    {
        return BinarySearch(0, _size, entity, null);
    }

    /// <summary>Searches the entire sorted <see cref="EntitySet&lt;TEntity&gt;"/> for an entity using the specified comparer and returns the zero-based index of the entity.</summary>
    /// <returns>The zero-based index of entity in the sorted <see cref="EntitySet&lt;TEntity&gt;"/>, if entity is found; otherwise, a negative number that is the bitwise complement of the index of the next entity that is larger than entity or, if there is no larger entity, the bitwise complement of <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</returns>
    /// <param name="entity">The object to locate. The value can be null for reference types.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing entities.<br/>-or-<br/>null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param>
    /// <exception cref="T:System.InvalidOperationException">comparer is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    public int BinarySearch(TEntity entity, IComparer<TEntity> comparer)
    {
        return BinarySearch(0, _size, entity, comparer);
    }

    /// <summary>Searches a range of entities in the sorted <see cref="EntitySet&lt;TEntity&gt;"/> for an entity using the specified comparer and returns the zero-based index of the entity.</summary>
    /// <returns>The zero-based index of entity in the sorted <see cref="EntitySet&lt;TEntity&gt;"/>, if entity is found; otherwise, a negative number that is the bitwise complement of the index of the next entity that is larger than entity or, if there is no larger entity, the bitwise complement of <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</returns>
    /// <param name="count">The length of the range to search.</param>
    /// <param name="entity">The object to locate. The value can be null for reference types.</param>
    /// <param name="index">The zero-based starting index of the range to search.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing entities, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/><br/>-or-<br/><br/>count is less than 0. </exception>
    /// <exception cref="T:System.InvalidOperationException">comparer is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    /// <exception cref="T:System.ArgumentException">index and count do not denote a valid range in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    public int BinarySearch(int index, int count, TEntity entity, IComparer<TEntity>? comparer)
    {
        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ArgumentOutOfRange_NeedNonNegNum);
        }
        if ((_size - index) < count)
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }
        return Array.BinarySearch<TEntity>(_items, index, count, entity, comparer);
    }

    /// <summary>
    /// Removes all entities from the <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public void Clear()
    {
        CheckReentrancy();
        ClearItems();
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionReset();
    }

    /// <summary>
    /// Removes all entities from the <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    void ClearItems()
    {
        Array.Clear(_items, 0, _size);
        _size = 0;
        _version++;
    }

    /// <summary>Determines whether an entity is in the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>true if entity is found in the <see cref="EntitySet&lt;TEntity&gt;"/>; otherwise, false.</returns>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    public bool Contains(TEntity? entity)
    {
        if (entity == null)
        {
            for (var j = 0; j < _size; j++)
            {
                if (_items[j] == null)
                {
                    return true;
                }
            }
            return false;
        }

        var items = _items;
        var size = _size;

        var comparer = EqualityComparer<TEntity>.Default;
        for (var i = 0; i < size; i++)
        {
            if (comparer.Equals(items[i], entity))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Converts the entities in the current <see cref="EntitySet&lt;TEntity&gt;"/> to another type implementing <see cref="IPersistEntity"/>, and returns a set containing the converted entities.</summary>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <param name="converter">A <see cref="T:System.Converter`2"/> delegate that converts each entity from one type to another type.</param>
    /// <returns>A <see cref="EntitySet&lt;TEntity&gt;"/> of the target type containing the converted entities from the current <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>        
    /// <exception cref="T:System.ArgumentNullException">converter is null.</exception>
    public EntitySet<TOutput> ConvertAll<TOutput>(Func<TEntity, TOutput> converter)
        where TOutput : IPersistEntity
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(converter);
#else
        if (converter == null)
        {
            throw new ArgumentNullException(nameof(converter));
        }
#endif
        var list = new EntitySet<TOutput>(_size);
        for (var i = 0; i < _size; i++)
        {
            list._items[i] = converter(_items[i]);
        }
        list._size = _size;
        return list;
    }

    /// <summary>Copies the entire <see cref="EntitySet&lt;TEntity&gt;"/> to a compatible one-dimensional array, starting at the beginning of the target array.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the entities copied from <see cref="EntitySet&lt;TEntity&gt;"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
    /// <exception cref="T:System.ArgumentException">The number of entities in the source <see cref="EntitySet&lt;TEntity&gt;"/> is greater than the number of entities that the destination array can contain.</exception>
    /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
    public void CopyTo(TEntity[] array)
    {
        CopyTo(array, 0);
    }

    /// <summary>Copies the entire <see cref="EntitySet&lt;TEntity&gt;"/> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the entities copied from <see cref="EntitySet&lt;TEntity&gt;"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <exception cref="T:System.ArgumentException">arrayIndex is equal to or greater than the length of array.<br/>-or-<br/>The number of entities in the source <see cref="EntitySet&lt;TEntity&gt;"/> is greater than the available space from arrayIndex to the end of the destination array.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
    /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
    public void CopyTo(TEntity[] array, int arrayIndex)
    {
        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    /// <summary>Copies a range of entities from the <see cref="EntitySet&lt;TEntity&gt;"/> to a compatible one-dimensional array, starting at the specified index of the target array.</summary>
    /// <param name="index">The zero-based index in the source <see cref="EntitySet&lt;TEntity&gt;"/> at which copying begins.</param>
    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the entities copied from <see cref="EntitySet&lt;TEntity&gt;"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>        
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>        
    /// <param name="count">The number of entities to copy.</param>/// 
    /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>arrayIndex is less than 0.<br/>-or-<br/>count is less than 0. </exception>
    /// <exception cref="T:System.ArgumentException">index is equal to or greater than the <see cref="EntitySet&lt;TEntity&gt;.Count"/> of the source <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>arrayIndex is equal to or greater than the length of array.<br/>-or-<br/>The number of entities from index to the end of the source <see cref="EntitySet&lt;TEntity&gt;"/> is greater than the available space from arrayIndex to the end of the destination array. </exception>
    public void CopyTo(int index, TEntity[] array, int arrayIndex, int count)
    {
        if ((_size - index) < count)
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }
        Array.Copy(_items, index, array, arrayIndex, count);
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            var num = (_items.Length == 0) ? 4 : (_items.Length * 2);
            if (num < min)
            {
                num = min;
            }
            Capacity = num;
        }
    }

    /// <summary>Determines whether the <see cref="EntitySet&lt;TEntity&gt;"/> contains entities that match the conditions defined by the specified predicate.</summary>
    /// <returns>true if the <see cref="EntitySet&lt;TEntity&gt;"/> contains one or more entities that match the conditions defined by the specified predicate; otherwise, false.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entities to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public bool Exists(Predicate<TEntity> match)
    {
        return (FindIndex(match) != -1);
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The first entity that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public TEntity? Find(Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException(nameof(match));
        }
#endif
        var items = _items;
        var size = _size;

        for (var i = 0; i < size; i++)
        {
            if (match(items[i]))
            {
                return items[i];
            }
        }
        return default;
    }

    private static TEntity[] FindAll(TEntity[] array, PersistChangeActions includedActions)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(array);
#else
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }
#endif
        return FindAll(array, array.Length - 1, match =>
        {
            return (match != null)
                && (((includedActions & PersistChangeActions.Delete) == PersistChangeActions.Delete && match.PersistChange == PersistChange.Delete)
                || ((includedActions & PersistChangeActions.Insert) == PersistChangeActions.Insert && match.PersistChange == PersistChange.Insert)
                || ((includedActions & PersistChangeActions.None) == PersistChangeActions.None && match.PersistChange == PersistChange.None)
                || ((includedActions & PersistChangeActions.Update) == PersistChangeActions.Update && match.PersistChange == PersistChange.Update));
        });
    }

    private static TEntity[] FindAll(TEntity[] array, int endIndex, Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(match);
#else
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }

        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif
        if (endIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(endIndex), ArgumentOutOfRange_Index);
        }

        List<TEntity> list = [];
        for (var i = 0; i <= endIndex; i++)
        {
            if (match(array[i]))
            {
                list.Add(array[i]);
            }
        }
        return [.. list];
    }

    /// <summary>Retrieves all the entities that match the conditions defined by the specified predicate.</summary>
    /// <returns>A <see cref="EntitySet&lt;TEntity&gt;"/> containing all the entities that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entities to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public EntitySet<TEntity> FindAll(Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var items = _items;
        var size = _size; ;

        EntitySet<TEntity> list = [];
        for (var i = 0; i < size; i++)
        {
            if (match(items[i]))
            {
                list.Add(items[i]);
            }
        }
        return list;
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The zero-based index of the first occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindIndex(Predicate<TEntity> match)
    {
        return FindIndex(0, _size, match);
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from the specified index to the last entity.</summary>
    /// <returns>The zero-based index of the first occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindIndex(int startIndex, Predicate<TEntity> match)
    {
        return FindIndex(startIndex, _size - startIndex, match);
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that starts at the specified index and contains the specified number of entities.</summary>
    /// <returns>The zero-based index of the first occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="startIndex">The zero-based starting index of the search.</param>
    /// <param name="count">The number of entities in the section to search.</param>        
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>count is less than 0.<br/>-or-<br/>startIndex and count do not specify a valid section in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindIndex(int startIndex, int count, Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var items = _items;
        var size = _size; ;

        if (startIndex > size)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), ArgumentOutOfRange_Index);
        }
        if ((count < 0) || (startIndex > (size - count)))
        {
            throw new ArgumentOutOfRangeException(nameof(count), ArgumentOutOfRange_Count);
        }

        var num = startIndex + count;
        for (var i = startIndex; i < num; i++)
        {
            if (match(items[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The last entity that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type TEntity.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public TEntity? FindLast(Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var items = _items;
        var size = _size; ;

        for (var i = size - 1; i >= 0; i--)
        {
            if (match(items[i]))
            {
                return items[i];
            }
        }

        return default;
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The zero-based index of the last occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindLastIndex(Predicate<TEntity> match)
    {
        return FindLastIndex(_size - 1, _size, match);
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from the first entity to the specified index.</summary>
    /// <returns>The zero-based index of the last occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindLastIndex(int startIndex, Predicate<TEntity> match)
    {
        return FindLastIndex(startIndex, startIndex + 1, match);
    }

    /// <summary>Searches for an entity that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that contains the specified number of entities and ends at the specified index.</summary>
    /// <returns>The zero-based index of the last occurrence of an entity that matches the conditions defined by match, if found; otherwise, –1.</returns>
    /// <param name="count">The number of entities in the section to search.</param>
    /// <param name="startIndex">The zero-based starting index of the backward search.</param>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entity to search for.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">startIndex is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>count is less than 0.<br/>-or-<br/>startIndex and count do not specify a valid section in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int FindLastIndex(int startIndex, int count, Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var items = _items;
        var size = _size; ;

        if (size == 0)
        {
            if (startIndex != -1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), ArgumentOutOfRange_Index);
            }
        }
        else if (startIndex >= size)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), ArgumentOutOfRange_Index);
        }
        if (count < 0 || ((startIndex - count) + 1) < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), ArgumentOutOfRange_Count);
        }

        var num = startIndex - count;
        for (var i = startIndex; i > num; i--)
        {
            if (match(items[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Performs the specified action on each entity of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="action">The <see cref="T:System.Action`1"/> delegate to perform on each entity of the <see cref="EntitySet&lt;TEntity&gt;"/>.</param>
    /// <exception cref="T:System.ArgumentNullException">action is null.</exception>
    public void ForEach(Action<TEntity> action)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(action);
#else
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
#endif

        var items = _items;
        var size = _size; ;

        for (var i = 0; i < size; i++)
        {
            action(items[i]);
        }
    }

    /// <summary>Returns an enumerator that iterates through the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>A <see cref="EntitySet&lt;TEntity&gt;.Enumerator"/> for the <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>Creates a shallow copy of a range of entities in the source <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>A shallow copy of a range of entities in the source <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    /// <param name="count">The number of entities in the range.</param>
    /// <param name="index">The zero-based <see cref="EntitySet&lt;TEntity&gt;"/> index at which the range starts.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>count is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">index and count do not denote a valid range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    public EntitySet<TEntity> GetRange(int index, int count)
    {
        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", ArgumentOutOfRange_NeedNonNegNum);
        }

        var items = _items;
        var size = _size; ;

        if ((size - index) < count)
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }

        var entitySet = new EntitySet<TEntity>(count);
        Array.Copy(items, index, entitySet._items, 0, count);
        entitySet._size = count;
        return entitySet;
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The zero-based index of the first occurrence of entity within the entire <see cref="EntitySet&lt;TEntity&gt;"/>, if found; otherwise, –1.</returns>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    public int IndexOf(TEntity entity)
    {
        var items = _items;
        var size = _size; ;
        return Array.IndexOf<TEntity>(items, entity, 0, size);
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from the specified index to the last entity.</summary>
    /// <returns>The zero-based index of the first occurrence of entity within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from index to the last entity, if found; otherwise, –1.</returns>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <param name="index">The zero-based starting index of the search.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    public int IndexOf(TEntity entity, int index)
    {
        var items = _items;
        var size = _size; ;

        if (index > size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), ArgumentOutOfRange_Index);
        }

        return Array.IndexOf<TEntity>(items, entity, index, size - index);
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that starts at the specified index and contains the specified number of entities.</summary>
    /// <returns>The zero-based index of the first occurrence of entity within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that starts at index and contains count number of entities, if found; otherwise, –1.</returns>
    /// <param name="count">The number of entities in the section to search.</param>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <param name="index">The zero-based starting index of the search.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>count is less than 0.<br/>-or-<br/>index and count do not specify a valid section in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    public int IndexOf(TEntity entity, int index, int count)
    {
        var items = _items;
        var size = _size; ;

        if (index > size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), ArgumentOutOfRange_Index);
        }
        if ((count < 0) || (index > (size - count)))
        {
            throw new ArgumentOutOfRangeException(nameof(count), ArgumentOutOfRange_Count);
        }

        return Array.IndexOf<TEntity>(items, entity, index, count);
    }

    /// <summary>Inserts an entity into the <see cref="EntitySet&lt;TEntity&gt;"/> at the specified index.</summary>
    /// <param name="entity">The object to insert. The value can be null for reference types.</param>
    /// <param name="index">The zero-based index at which entity should be inserted.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</exception>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public void Insert(int index, TEntity entity)
    {
        CheckReentrancy();
        InsertItem(index, entity);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, entity, index);
    }

    /// <summary>Inserts an entity into the <see cref="EntitySet&lt;TEntity&gt;"/> at the specified index.</summary>
    /// <param name="entity">The object to insert. The value can be null for reference types.</param>
    /// <param name="index">The zero-based index at which entity should be inserted.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</exception>
    void InsertItem(int index, TEntity entity)
    {
        if (index > _size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), ArgumentOutOfRange_ListInsert);
        }

        if (_size == _items.Length)
        {
            EnsureCapacity(_size + 1);
        }
        if (index < _size)
        {
            Array.Copy(_items, index, _items, index + 1, _size - index);
        }
        _items[index] = entity;
        _size++;
        _version++;
    }

    /// <summary>Inserts the entities of a collection into the <see cref="EntitySet&lt;TEntity&gt;"/> at the specified index.</summary>
    /// <param name="collection">The collection whose entities should be inserted into the <see cref="EntitySet&lt;TEntity&gt;"/>. The collection itself cannot be null, but it can contain entities that are null, if type TEntity is a reference type.</param>
    /// <param name="index">The zero-based index at which the new entities should be inserted.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</exception>
    /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
    public void InsertRange(int index, IEnumerable<TEntity> collection)
    {
        if (index > _size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), ArgumentOutOfRange_Index);
        }
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(collection);
#else
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
#endif

        if (collection is ICollection<TEntity> is2)
        {
            var count = is2.Count;
            if (count > 0)
            {
                EnsureCapacity(_size + count);
                if (index < _size)
                {
                    Array.Copy(_items, index, _items, index + count, _size - index);
                }
                if (this == is2)
                {
                    Array.Copy(_items, 0, _items, index, index);
                    Array.Copy(_items, index + count, _items, index * 2, _size - index);
                }
                else
                {
                    var array = new TEntity[count];
                    is2.CopyTo(array, 0);
                    array.CopyTo(_items, index);
                }
                _size += count;
            }
        }
        else
        {
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                InsertItem(index++, enumerator.Current);
            }
        }

        _version++;
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private static bool IsCompatibleObject(object value)
    {
        if (value is not TEntity && ((value != null) || typeof(TEntity).GetTypeInfo().IsValueType))
        {
            return false;
        }
        return true;
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the entire <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The zero-based index of the last occurrence of entity within the entire the <see cref="EntitySet&lt;TEntity&gt;"/>, if found; otherwise, –1.</returns>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    public int LastIndexOf(TEntity entity)
    {
        return LastIndexOf(entity, _size - 1, _size);
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from the first entity to the specified index.</summary>
    /// <returns>The zero-based index of the last occurrence of entity within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that extends from the first entity to index, if found; otherwise, –1.</returns>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>. </exception>
    public int LastIndexOf(TEntity entity, int index)
    {
        if (index >= _size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), ArgumentOutOfRange_Index);
        }
        return LastIndexOf(entity, index, index + 1);
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that contains the specified number of entities and ends at the specified index.</summary>
    /// <returns>The zero-based index of the last occurrence of entity within the range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/> that contains count number of entities and ends at index, if found; otherwise, –1.</returns>
    /// <param name="count">The number of entities in the section to search.</param>
    /// <param name="entity">The object to locate in the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is outside the range of valid indexes for the <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>count is less than 0.<br/>-or-<br/>index and count do not specify a valid section in the <see cref="EntitySet&lt;TEntity&gt;"/>. </exception>
    public int LastIndexOf(TEntity entity, int index, int count)
    {
        var items = _items;
        var size = _size; ;

        if (size == 0)
        {
            return -1;
        }

        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", ArgumentOutOfRange_NeedNonNegNum);
        }
        if ((index >= size) || (count > (index + 1)))
        {
            throw new ArgumentOutOfRangeException((index >= size) ? "index" : "count", ArgumentOutOfRange_BiggerThanCollection);
        }

        return Array.LastIndexOf<TEntity>(items, entity, index, count);
    }

    /// <summary>Removes the first occurrence of a specific object from the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>true if entity is successfully removed; otherwise, false.  This method also returns false if entity was not found in the <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    /// <param name="entity">The object to remove from the <see cref="EntitySet&lt;TEntity&gt;"/>. The value can be null for reference types.</param>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public bool Remove(TEntity entity)
    {
        var index = IndexOf(entity);
        if (index >= 0)
        {
            RemoveItemAt(index);
            return true;
        }
        return false;
    }

    /// <summary>Removes the all the entities that match the conditions defined by the specified predicate.</summary>
    /// <returns>The number of entities removed from the <see cref="EntitySet&lt;TEntity&gt;"/> .</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions of the entities to remove.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public int RemoveAll(Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var count = 0;
        for (var i = _size - 1; i >= 0; i--)
        {
            var entity = _items[i];
            if (match(entity))
            {
                if (entity == null)
                {
                    BaseRemoveAt(i);
                }
                else
                {
                    switch (entity.PersistChange)
                    {
                        case PersistChange.None:
                        case PersistChange.Update:
                            entity.PersistChange = PersistChange.Delete;
                            break;
                        case PersistChange.Insert:
                            BaseRemoveAt(i);
                            break;
                        case PersistChange.Delete:
                            break;
                        default: //could never happen
                            throw new NotSupportedException();
                    }
                }
                count++;
            }
        }
        _version++;
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return count;
    }

    /// <summary>Removes the entity at the specified index of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="index">The zero-based index of the entity to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is equal to or greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</exception>
    /// <remarks>
    /// <para>This implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public void RemoveAt(int index)
    {
        RemoveItemAt(index);
    }

    /// <summary>Removes the entity at the specified index of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="index">The zero-based index of the entity to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is equal to or greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>.</exception>
    void RemoveItemAt(int index)
    {
        CheckReentrancy();
        var entity = this[index];

#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
#else
        if (index >= _size)
        {
            throw new ArgumentOutOfRangeException();
        }
#endif
        if (_items[index] == null)
        {
            BaseRemoveAt(index);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(ItemPropertyName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, entity, index);
        }
        else
        {
            switch (entity.PersistChange)
            {
                case PersistChange.None:
                case PersistChange.Update:
                    _items[index].PersistChange = PersistChange.Delete;
                    _version++;
                    break;
                case PersistChange.Insert:
                    BaseRemoveAt(index);
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(ItemPropertyName);
                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, entity, index);
                    break;
                case PersistChange.Delete:
                    break;
                default: //could never happen
                    throw new NotSupportedException();
            }
        }
    }

    /// <summary>Removes a range of entities from the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <param name="index">The zero-based starting index of the range of entities to remove.</param>
    /// <param name="count">The number of entities to remove.</param>        
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>count is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">index and count do not denote a valid range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/>.</exception>
    public void RemoveRange(int index, int count)
    {
        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", ArgumentOutOfRange_NeedNonNegNum);
        }

        if ((_size - index) < count) // because we must have index + count <= size
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }

        for (var i = index; i < index + count; i++)
        {
            if (_items[i] == null)
            {
                BaseRemoveAt(i);
            }
            else
            {
                switch (_items[i].PersistChange)
                {
                    case PersistChange.None:
                    case PersistChange.Update:
                        _items[i].PersistChange = PersistChange.Delete;
                        _version++;
                        break;
                    case PersistChange.Insert:
                        BaseRemoveAt(i);
                        break;
                    case PersistChange.Delete:
                        break;
                    default: //could never happen
                        throw new NotSupportedException();
                }
            }
        }

        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    }

    /// <summary>
    /// Reverses the order of the entities in the entire <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    public void Reverse()
    {
        Reverse(0, Count);
    }

    /// <summary>Reverses the order of the entities in the specified range.</summary>
    /// <param name="index">The zero-based starting index of the range to reverse.</param>
    /// <param name="count">The number of entities in the range to reverse.</param>        
    /// <exception cref="T:System.ArgumentException">index and count do not denote a valid range of entities in the <see cref="EntitySet&lt;TEntity&gt;"/>. </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>count is less than 0. </exception>
    public void Reverse(int index, int count)
    {
        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", ArgumentOutOfRange_NeedNonNegNum);
        }
        if ((_size - index) < count)
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }
        Array.Reverse(_items, index, count);
        _version++;
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>Sorts the entities in the entire <see cref="EntitySet&lt;TEntity&gt;"/> using the default comparer.</summary>
    /// <exception cref="T:System.InvalidOperationException">The default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find an implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    public void Sort()
    {
        Sort(0, Count, null);
    }

    /// <summary>Sorts the entities in the entire <see cref="EntitySet&lt;TEntity&gt;"/> using the specified comparer.</summary>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing entities, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param>
    /// <exception cref="T:System.ArgumentException">The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an entity with itself.</exception>
    /// <exception cref="T:System.InvalidOperationException">comparer is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    public void Sort(IComparer<TEntity> comparer)
    {
        Sort(0, Count, comparer);
    }

    /// <summary>Sorts the entities in the entire <see cref="EntitySet&lt;TEntity&gt;"/> using the specified <see cref="T:System.Comparison`1"/>.</summary>
    /// <param name="comparison">The <see cref="T:System.Comparison`1"/> to use when comparing entities.</param>
    /// <exception cref="T:System.ArgumentException">The implementation of comparison caused an error during the sort. For example, comparison might not return 0 when comparing an entity with itself.</exception>
    /// <exception cref="T:System.ArgumentNullException">comparison is null.</exception>
    public void Sort(Comparison<TEntity> comparison)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(comparison);
#else
        if (comparison == null)
        {
            throw new ArgumentNullException(nameof(comparison));
        }
#endif
        if (_size > 0)
        {
            IComparer<TEntity> comparer = new FunctorComparer<TEntity>(comparison);
            Array.Sort<TEntity>(_items, 0, _size, comparer);
            _version++;
            OnPropertyChanged(ItemPropertyName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>Sorts the entities in a range of entities in <see cref="EntitySet&lt;TEntity&gt;"/> using the specified comparer.</summary>
    /// <param name="index">The zero-based starting index of the range to sort.</param>
    /// <param name="count">The length of the range to sort.</param>        
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1"/> implementation to use when comparing entities, or null to use the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.</param>
    /// <exception cref="T:System.ArgumentException">index and count do not specify a valid range in the <see cref="EntitySet&lt;TEntity&gt;"/>.<br/>-or-<br/>The implementation of comparer caused an error during the sort. For example, comparer might not return 0 when comparing an entity with itself.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>count is less than 0.</exception>
    /// <exception cref="T:System.InvalidOperationException">comparer is null, and the default comparer <see cref="P:System.Collections.Generic.Comparer`1.Default"/> cannot find implementation of the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface for type TEntity.</exception>
    public void Sort(int index, int count, IComparer<TEntity>? comparer)
    {
        if ((index < 0) || (count < 0))
        {
            throw new ArgumentOutOfRangeException((index < 0) ? nameof(index) : nameof(count), ArgumentOutOfRange_NeedNonNegNum);
        }
        if ((_size - index) < count)
        {
            throw new ArgumentException(Argument_InvalidOffLen);
        }
        Array.Sort<TEntity>(_items, index, count, comparer);
        _version++;
        OnPropertyChanged(ItemPropertyName);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(array);
#else
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }
#endif
        if ((array != null) && (array.Rank != 1))
        {
            throw new ArgumentException(Arg_RankMultiDimNotSupported);
        }
        try
        {
            Array.Copy(_items, 0, array!, arrayIndex, _size);
        }
        catch (ArrayTypeMismatchException)
        {
            throw new ArgumentException(Argument_InvalidArrayType);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    int IList.Add(object? item)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#else
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
#endif
        EntitySet<TEntity>.VerifyValueType(item);
        Add((TEntity)item);
        return (Count - 1);
    }

    bool IList.Contains(object? entity)
    {
        if (null == entity)
        {
            return false;
        }
        return (EntitySet<TEntity>.IsCompatibleObject(entity) && Contains((TEntity)entity));
    }

    int IList.IndexOf(object? entity)
    {
        if (null == entity)
        {
            return -1;
        }

        if (EntitySet<TEntity>.IsCompatibleObject(entity))
        {
            return IndexOf((TEntity)entity);
        }
        return -1;
    }

    void IList.Insert(int index, object? entity)
    {
        EntitySet<TEntity>.VerifyValueType(entity);
        Insert(index, (TEntity)entity!);
    }

    void IList.Remove(object? entity)
    {
        if (null == entity)
        {
            return;
        }

        if (EntitySet<TEntity>.IsCompatibleObject(entity))
        {
            Remove((TEntity)entity);
        }
    }

    /// <summary>Copies the entities of the <see cref="EntitySet&lt;TEntity&gt;"/> to a new array.</summary>
    /// <returns>An array containing copies of the entities of the <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    public TEntity[] ToArray()
    {
        var destinationArray = new TEntity[_size];
        Array.Copy(_items, 0, destinationArray, 0, _size);
        return destinationArray;
    }

    /// <summary>Copies the entities matching the included actions to a new array.</summary>
    /// <param name="includedActions">One or a combination of the <see cref="PersistChangeActions"/> values.</param>
    /// <returns>An array containing copies of the entities matching the specified actions.</returns>
    public TEntity[] ToArray(PersistChangeActions includedActions)
    {
        var sourceArray = FindAll(_items, includedActions);
        var destinationArray = new TEntity[sourceArray.Length];
        Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
        return destinationArray;
    }

    /// <summary>Sets the capacity to the actual number of entities in the <see cref="EntitySet&lt;TEntity&gt;"/>, if that number is less than a threshold value.</summary>
    /// <remarks>
    /// <para>This method can be used to minimize a collection's memory overhead if no new entities will be added to the collection. The cost of reallocating and copying a large <see cref="EntitySet&lt;TEntity&gt;"/> can be considerable, however, so the TrimExcess method does nothing if the list is at more than 90 percent of capacity. This avoids incurring a large reallocation cost for a relatively small gain.</para>
    /// </remarks>
    public void TrimExcess()
    {
        var num = (int)(_items.Length * 0.9);
        if (_size < num)
        {
            Capacity = _size;
        }
    }

    /// <summary>Determines whether every entity in the <see cref="EntitySet&lt;TEntity&gt;"/> matches the conditions defined by the specified predicate.</summary>
    /// <returns>true if every entity in the <see cref="EntitySet&lt;TEntity&gt;"/> matches the conditions defined by the specified predicate; otherwise, false. If the set has no entities, the return value is true.</returns>
    /// <param name="match">The <see cref="T:System.Predicate`1"/> delegate that defines the conditions to check against the entities.</param>
    /// <exception cref="T:System.ArgumentNullException">match is null.</exception>
    public bool TrueForAll(Predicate<TEntity> match)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(match);
#else
        if (match == null)
        {
            throw new ArgumentNullException("match");
        }
#endif

        var items = _items;
        var size = _size; ;

        for (var i = 0; i < size; i++)
        {
            if (!match(items[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void BaseRemoveAt(int index)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);
#else
        if (index >= _size)
        {
            throw new ArgumentOutOfRangeException();
        }
#endif
        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }
        _items[_size] = default!;
        _version++;
    }

    private static void VerifyValueType(object? value)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value);
#else
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }
#endif
        if (!EntitySet<TEntity>.IsCompatibleObject(value))
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Arg_WrongType, value, typeof(TEntity)), nameof(value));
        }
    }

    /// <summary>Gets or sets the total number of entities the internal data structure can hold without resizing.</summary>
    /// <returns>The number of entities that the <see cref="EntitySet&lt;TEntity&gt;"/> can contain before resizing is required.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><see cref="EntitySet&lt;TEntity&gt;.Capacity"/> is set to a value that is less than <see cref="Count"/>. </exception>
    public int Capacity
    {
        get
        {
            return _items.Length;
        }
        set
        {
            if (value != _items.Length)
            {
                if (value < _size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), ArgumentOutOfRange_SmallCapacity);
                }
                if (value > 0)
                {
                    var destinationArray = new TEntity[value];
                    if (_size > 0)
                    {
                        Array.Copy(_items, 0, destinationArray, 0, _size);
                    }
                    _items = destinationArray;
                }
                else
                {
                    _items = EntitySet<TEntity>._emptyArray;
                }
            }
        }
    }

    /// <summary>Gets the number of entities actually contained in the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
    /// <returns>The number of entities actually contained in the <see cref="EntitySet&lt;TEntity&gt;"/>.</returns>
    public int Count
    {
        get
        {
            return _size;
        }
    }

    /// <summary>Gets or sets the entity at the specified index.</summary>
    /// <returns>The entity at the specified index.</returns>
    /// <param name="index">The zero-based index of the entity to get or set.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than 0.<br/>-or-<br/>index is equal to or greater than <see cref="EntitySet&lt;TEntity&gt;.Count"/>. </exception>
    /// <remarks>
    /// <para>The set implementation raises the <see cref="CollectionChanged"/> event.</para>
    /// </remarks>
    public TEntity this[int index]
    {
        get
        {
            var items = _items;
            var size = _size; ;

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
#else
            if (index >= size)
            {
                throw new ArgumentOutOfRangeException();
            }
#endif
            return items[index];
        }
        set
        {
            CheckReentrancy();
            var oldItem = this[index];
            SetItem(index, value);
            OnPropertyChanged(ItemPropertyName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, value, index);
        }
    }

    /// <summary>
    /// Replaces the entity at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the entity to replace.</param>
    /// <param name="entity">The new value for the entity at the specified index.</param>
    void SetItem(int index, TEntity entity)
    {
        var items = _items;
        var size = _size; ;

#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, size);
#else
        if (index >= size)
        {
            throw new ArgumentOutOfRangeException();
        }
#endif
        items[index] = entity;
        _version++;
    }

    bool ICollection<TEntity>.IsReadOnly
    {
        get
        {
            return false;
        }
    }

    bool ICollection.IsSynchronized
    {
        get
        {
            return false;
        }
    }
    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
    /// </returns>
    public object? SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                Interlocked.CompareExchange(ref _syncRoot, new object(), null);
            }
            return _syncRoot;
        }
    }

    object ICollection.SyncRoot
    {
        get
        {
            if (_syncRoot == null)
            {
                Interlocked.CompareExchange(ref _syncRoot, new object(), null);
            }
            return _syncRoot;
        }
    }

    bool IList.IsFixedSize
    {
        get
        {
            return false;
        }
    }

    bool IList.IsReadOnly
    {
        get
        {
            return false;
        }
    }

    object? IList.this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(value);
#else
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
#endif
            EntitySet<TEntity>.VerifyValueType(value);
            this[index] = (TEntity)value;
        }
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="EntitySet&lt;TEntity&gt;"/> to EntitySet&lt;EntityItem&lt;TEntity&gt;&gt;/>.
    /// </summary>
    /// <param name="entities">The entities.</param>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <c>null</c>.</exception>
    public static implicit operator EntitySet<EntityItem<TEntity>>(EntitySet<TEntity> entities)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(entities);
#else
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }
#endif
        return entities.ConvertAll<EntityItem<TEntity>>(converter => (EntityItem<TEntity>)converter);
    }

    /// <summary>
    /// Performs an implicit conversion from EntitySet&lt;EntityItem&lt;TEntity&gt;&gt; to <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>The result of the conversion.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <c>null</c>.</exception>
    public static implicit operator EntitySet<TEntity>(EntitySet<EntityItem<TEntity>> items)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(items);
#else
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
#endif
        return items.ConvertAll<TEntity>(converter => converter!);
    }

    /// <summary>
    /// Enumerates the entities of a <see cref="EntitySet&lt;TEntity&gt;"/>.
    /// </summary>

    [StructLayout(LayoutKind.Sequential)]
    public struct Enumerator : IEnumerator<TEntity>, IDisposable, IEnumerator
    {
        private readonly EntitySet<TEntity> list;
        private int _index;
        private readonly int _version;
        private TEntity? _current;

        internal Enumerator(EntitySet<TEntity> list)
        {
            this.list = list;
            _index = 0;
            _version = list._version;
            _current = default;
        }

        /// <summary>Releases all resources used by the <see cref="EntitySet&lt;TEntity&gt;.Enumerator"/>.</summary>
        public readonly void Dispose()
        {
        }

        /// <summary>Advances the enumerator to the next entity of the <see cref="EntitySet&lt;TEntity&gt;"/>.</summary>
        /// <returns>true if the enumerator was successfully advanced to the next entity; false if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            if (_version != list._version)
            {
                throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
            }

            if (_index < list._size)
            {
                _current = list._items[_index];
                _index++;
                return true;
            }

            _index = list._size + 1;
            _current = default;
            return false;
        }

        /// <summary>Gets the entity at the current position of the enumerator.</summary>
        /// <returns>The entity in the <see cref="EntitySet&lt;TEntity&gt;"/> at the current position of the enumerator.</returns>
        public readonly TEntity Current
        {
            get
            {
                return _current!;
            }
        }

        readonly object IEnumerator.Current
        {
            get
            {
                if ((_index == 0) || (_index == (list._size + 1)))
                {
                    throw new InvalidOperationException(InvalidOperation_EnumOpCantHappen);
                }
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            if (_version != list._version)
            {
                throw new InvalidOperationException(InvalidOperation_EnumFailedVersion);
            }
            _index = 0;
            _current = default;
        }
    }

    internal sealed class FunctorComparer<T>(Comparison<T> comparison) : IComparer<T>
    {
        public int Compare(T? x, T? y)
        {
            if (x == null)
            {
                return (y == null) ? 0 : -1;
            }
            if (y == null)
            {
                return 1;
            }
            return comparison(x, y);
        }
    }

    private sealed class SimpleMonitor : IDisposable
    {
        private int _busyCount;

        public void Dispose()
        {
            _busyCount--;
        }

        public void Enter()
        {
            _busyCount++;
        }

        public bool Busy
        {
            get
            {
                return (_busyCount > 0);
            }
        }

        public SimpleMonitor()
        {

        }
    }
}

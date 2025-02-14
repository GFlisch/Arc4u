namespace Arc4u.Data;

/// <summary>
/// Extends <see cref="EntitySet&lt;TEntity&gt;"/> type.
/// </summary>
public static class EntitySetExtension
{
    /// <summary>
    /// Gets the number of entities actually contained in the <see cref="EntitySet&lt;TEntity&gt;"/>
    /// matching the specified <see cref="PersistChangeActions"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="includedActions">The included actions.</param>
    /// <returns>The number of matching entities.</returns>
    public static int Count<TEntity>(this EntitySet<TEntity> source, PersistChangeActions includedActions)
        where TEntity : IPersistEntity
    {
        return source.ToArray(includedActions).Length;
    }
}

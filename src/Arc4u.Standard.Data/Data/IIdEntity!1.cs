namespace Arc4u.Data
{
    /// <summary>
    /// The interface a class must implement if it requires an unique identifier.
    /// </summary>
    /// <typeparam name="TId">The type of identification.</typeparam>
    public interface IIdEntity<TId>
    {
        /// <summary>
        /// Gets or sets the <see cref="T:System.Object"/> identifier.
        /// </summary>
        /// <value>The <see cref="T:System.Object"/> Id.</value>
        TId Id { get; }
    }
}

namespace Arc4u.Data
{
    /// <summary>
    /// The interface an entity must implement if it requires change tracking.
    /// </summary>
    public interface IPersistEntity
    {
        /// <summary>
        /// Gets or sets the persist change.
        /// </summary>
        /// <value>The persist change.</value>
        PersistChange PersistChange { get; set; }
    }
}
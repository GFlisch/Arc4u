
namespace Arc4u.Data
{
    /// <summary>
    /// The interface a class must implement if it requires audit of creation information.
    /// </summary>
    public interface ICreateAuditEntity<TCreatedBy, TCreatedOn>
    {
        /// <summary>
        /// Gets or sets the user who has created the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <value>The creator.</value>
        TCreatedBy CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the creation date and time of the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <value>The creation date and time.</value>
        TCreatedOn CreatedOn { get; set; }
    }
}
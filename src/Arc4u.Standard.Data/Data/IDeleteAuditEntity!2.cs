
namespace Arc4u.Data
{
    /// <summary>
    /// The interface a class must implement if it requires audit of delete information.
    /// </summary>
    public interface IDeleteAuditEntity<TDeletedBy, TDeletedOn>
    {
        /// <summary>
        /// Gets or sets the user who has deleted the current <see cref="T:System.Object"/>. 
        /// </summary>
        /// <value>The delete user.</value>
        TDeletedBy DeletedBy { get; set; }

        /// <summary>
        /// Gets or sets the delete date and time of the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <value>The delete date and time.</value>
        TDeletedOn DeletedOn { get; set; }
    }
}

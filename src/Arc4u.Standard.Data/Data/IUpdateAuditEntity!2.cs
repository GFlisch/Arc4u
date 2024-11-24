
namespace Arc4u.Data;

/// <summary>
/// The interface a class must implement if it requires audit of update information.
/// </summary>
public interface IUpdateAuditEntity<TUpdatedBy, TUpdatedOn>
{
    /// <summary>
    /// Gets or sets the last user who has updated the current <see cref="T:System.Object"/>. 
    /// </summary>
    /// <value>The last updater.</value>
    TUpdatedBy UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last update date and time of the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <value>The last update date and time.</value>
    TUpdatedOn UpdatedOn { get; set; }
}

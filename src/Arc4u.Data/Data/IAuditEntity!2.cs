
namespace Arc4u.Data;

/// <summary>
/// Defines an interface for entities that track audit information,
/// specifying who performed an action and when it was performed.
/// </summary>
/// <typeparam name="TAuditedBy">The type representing the user or system that performed the action.</typeparam>
/// <typeparam name="TAuditedOn">The type representing the timestamp of when the action was performed.</typeparam>
public interface IAuditEntity<TAuditedBy, TAuditedOn>
{
    /// <summary>
    /// Gets or sets the identifier of the user or system that performed the action.
    /// </summary>
    TAuditedBy AuditedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp indicating when the action was performed.
    /// </summary>
    TAuditedOn AuditedOn { get; set; }
}

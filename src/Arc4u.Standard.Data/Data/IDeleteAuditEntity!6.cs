namespace Arc4u.Data;

/// <summary>
/// The interface a class must implement if it requires audit of creation, update and delete information.
/// </summary>
public interface IDeleteAuditEntity<TCreatedBy, TCreatedOn, TUpdatedBy, TUpdatedOn, TDeletedBy, TDeletedOn>
    : ICreateAuditEntity<TCreatedBy, TCreatedOn>
    , IUpdateAuditEntity<TUpdatedBy, TUpdatedOn>
    , IDeleteAuditEntity<TDeletedBy, TDeletedOn>
{
}

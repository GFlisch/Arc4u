namespace Arc4u.Data
{
    /// <summary>
    /// The interface a class must implement if it requires audit of creation and update information.
    /// </summary>
    public interface IUpdateAuditEntity<TCreatedBy, TCreatedOn, TUpdatedBy, TUpdatedOn>
        : ICreateAuditEntity<TCreatedBy, TCreatedOn>
        , IUpdateAuditEntity<TUpdatedBy, TUpdatedOn>
    {
    }
}
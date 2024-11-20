using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Represents a <see cref="PersistEntity"/> 
    /// identified with a <see cref="System.Guid"/>,
    /// auditing its creation information with <see cref="System.string"/> and <see cref="System.DateTimeOffset"/>.
    /// </summary>
    /// <remarks>Storage members are marked as protected in order to enable LinqToSql mappings.</remarks>

    [DataContract]
    public class CreateAuditEntity : CreateAuditEntity<Guid, string, DateTimeOffset>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAuditEntity"/> class 
        /// <see cref="PersistChange"/> is defaulted to None.
        /// </summary>
        public CreateAuditEntity() : this(PersistChange.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAuditEntity"/> class 
        /// with the specified <see cref="PersistChange"/> 
        /// </summary>
        /// <param name="persistChange">The persist change.</param>
        public CreateAuditEntity(PersistChange persistChange) : base(persistChange)
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.UtcNow;
        }

        #endregion
    }
}

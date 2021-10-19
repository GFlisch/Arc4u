using System;
using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Represents a <see cref="PersistEntity"/> 
    /// identified with a <see cref="System.Guid"/>,
    /// auditing its creation and update information with <see cref="System.String"/> and <see cref="System.DateTimeOffset"/>.
    /// </summary>    
    /// <remarks>Storage members are marked as protected in order to enable LinqToSql mappings.</remarks>
    [DataContract]
    public class UpdateAuditEntity : UpdateAuditEntity<Guid, string, DateTimeOffset, string, DateTimeOffset?>
    {
        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAuditEntity"/> class 
        /// or with an enabled <see cref="ChangeTracking"/>.
        /// </summary>
        public UpdateAuditEntity() : this(PersistChange.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAuditEntity"/> class 
        /// with the specified <see cref="PersistChange"/> 
        /// </summary>
        /// <param name="persistChange">The persist change.</param>
        public UpdateAuditEntity(PersistChange persistChange)
            : base(persistChange)
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.UtcNow;
        }


        #endregion
    }
}
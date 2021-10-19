using System;
using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Represents a <see cref="PersistEntity"/> identified with a <see cref="System.Guid"/>.
    /// </summary>
    /// <remarks>Storage members are marked as protected in order to enable mapping with LinqToSql.</remarks>

    [DataContract]
    public class IdEntity : IdEntity<Guid>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdEntity"/> class
        /// <see cref="PersistChange"/> is defaulted to None.
        /// </summary>
        public IdEntity() : this(PersistChange.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdEntity"/> class 
        /// with the specified <see cref="PersistChange"/> 
        /// </summary>
        /// <param name="persistChange">The persist change.</param>
        protected IdEntity(PersistChange persistChange)
            : base(persistChange)
        {
            Id = Guid.NewGuid();
        }

        #endregion
    }
}
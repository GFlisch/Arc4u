using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Describes the type of change a class implementing <see cref="IPersistEntity"/> will undergo when changes are submitted to the database.
    /// </summary>
    [DataContract]
    public enum PersistChange
    {
        /// <summary>
        /// The entity will not be changed.
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// The entity will be deleted.
        /// </summary>
        [EnumMember]
        Delete = 1,
        /// <summary>
        /// The entity will be inserted.
        /// </summary>
        [EnumMember]
        Insert = 2,
        /// <summary>
        /// The entity will be updated.
        /// </summary>
        [EnumMember]
        Update = 3
    }
}
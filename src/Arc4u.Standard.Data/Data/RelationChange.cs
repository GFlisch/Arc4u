using System.Runtime.Serialization;

namespace Arc4u.Data
{
    /// <summary>
    /// Describes the type of change in the relation with another object.
    /// </summary>
    [DataContract]
    public enum RelationChange
    {
        /// <summary>
        /// The relation will not be submitted.
        /// </summary>
        [EnumMember]
        None = 0,
        /// <summary>
        /// The relation will be deleted.
        /// </summary>
        [EnumMember]
        Delete = 1,
        /// <summary>
        /// The relation will be inserted.
        /// </summary>
        [EnumMember]
        Insert = 2,
    }
}

using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the possible types of a <see cref="Bound&lt;T&gt;"/>.
    /// </summary>
    [DataContract]
    public enum BoundType
    {
        /// <summary>
        /// Indicates a lower <see cref="Bound&lt;T&gt;"/>.
        /// </summary>
        [EnumMember]
        Lower = 0,
        /// <summary>
        /// Indicates an upper <see cref="Bound&lt;T&gt;"/>.
        /// </summary>
        [EnumMember]
        Upper = 1,
    }
}

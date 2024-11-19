using System.Runtime.Serialization;

namespace Arc4u.Diagnostics
{
    /// <summary>
    /// Specifies the different categories of a <see cref="FaultMessage"/>.
    /// </summary>
    [DataContract]
    [Flags]
    public enum MessageCategory : short
    {
        /// <summary>
        /// A message dedicates for IT people.
        /// </summary>
        [EnumMember]
        Technical = 1,
        /// <summary>
        /// A message dedicates for IT and Business people.
        /// </summary>
        [EnumMember]
        Business = 2,

        /// <summary>
        /// For Monitoring purpose.
        /// </summary>
        [EnumMember]
        Monitoring = 4,
    }
}

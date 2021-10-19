using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the week position of the month.
    /// </summary>
    [DataContract]
    public enum WeekOfMonthPosition : short
    {
        /// <summary>
        /// The first week of the month.
        /// </summary>
        [EnumMember]
        First = 1,
        /// <summary>
        /// The second week of the month.
        /// </summary>
        [EnumMember]
        Second = 2,
        /// <summary>
        /// The third week of the month.
        /// </summary>
        [EnumMember]
        Third = 4,
        /// <summary>
        /// The fourth week of the month.
        /// </summary>
        [EnumMember]
        Fourth = 8,
        /// <summary>
        /// The last week of the month.
        /// </summary>
        [EnumMember]
        Last = 16,
    }
}
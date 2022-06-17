using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the recurrent days of week.
    /// </summary>
    [Flags]
    [DataContract]
    public enum RecurrentWeekDays : short
    {
        /// <summary>
        /// Occurs every Sunday.
        /// </summary>
        [EnumMember]
        Sunday = 1,
        /// <summary>
        /// Occurs every Monday.
        /// </summary>
        [EnumMember]
        Monday = 2,
        /// <summary>
        /// Occurs every Tuesday.
        /// </summary>
        [EnumMember]
        Tuesday = 4,
        /// <summary>
        /// Occurs every Wednesday.
        /// </summary>
        [EnumMember]
        Wednesday = 8,
        /// <summary>
        /// Occurs every Thursday.
        /// </summary>
        [EnumMember]
        Thursday = 16,
        /// <summary>
        /// Occurs every Friday.
        /// </summary>
        [EnumMember]
        Friday = 32,
        /// <summary>
        /// Occurs every Saturday.
        /// </summary>
        [EnumMember]
        Saturday = 64,
        /// <summary>
        /// Occurs every Monday, Tuesday, Wednesday, Thursday, Friday.
        /// </summary>
        [EnumMember]
        WorkWeek = 62,
        /// <summary>
        /// Occurs every Saturday, Sunday.
        /// </summary>
        [EnumMember]
        WeekEnd = 65,
        /// <summary>
        /// Occurs every Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, and Sunday.
        /// </summary>
        [EnumMember]
        EveryDay = 127,
    }
}

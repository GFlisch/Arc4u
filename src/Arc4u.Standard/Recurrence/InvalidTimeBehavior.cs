using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the behavior of a <see cref="Recurrence"/> regarding an invalid time.
    /// An invalid time is a nonexistent time that is an artifact of the transition from standard time to daylight saving time. 
    /// It occurs when the clock time is adjusted forward in time, such as during the transition from a time zone's standard time to its daylight saving time. 
    /// For example, if this transition occurs on a particular day at 2:00 A.M. and causes the time to change to 3:00 A.M., 
    /// each time interval between 2:00 A.M. and 2:59:99 A.M. is invalid.
    /// </summary>
    [DataContract]
    public enum InvalidTimeBehavior : short
    {
        /// <summary>
        /// The <see cref="Recurrence"/> shifts occurrences scheduled on invalid time to the daylight saving time.
        /// </summary>
        [EnumMember]
        Shift = 1,
        /// <summary>
        /// The <see cref="Recurrence"/> skips occurrences scheduled on invalid time.
        /// </summary>
        [EnumMember]
        Skip = 2,
    }
}

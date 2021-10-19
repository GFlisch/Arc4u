using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the behavior of a <see cref="Recurrence"/> regarding an ambiguous time.
    /// An ambiguous time is a time that can be mapped to two different times in a single time zone. 
    /// It occurs when the clock time is adjusted back in time, such as during the transition from a time zone's daylight saving time to its standard time. 
    /// For example, if this transition occurs on a particular day at 2:00 A.M. and causes the time to change to 1:00 A.M., 
    /// each time interval between 1:00 A.M. and 1:59:99 A.M. can be interpreted as either a standard time or a daylight saving time. 
    /// </summary>
    [DataContract]
    public enum AmbiguousTimeBehavior : short
    {
        /// <summary>
        /// The <see cref="Recurrence"/> executes occurences only on standard time.
        /// </summary>
        [EnumMember]
        StandardTime = 1,
        /// <summary>
        /// The <see cref="Recurrence"/> executes occurences only on daylight saving time.
        /// </summary>
        [EnumMember]
        DaylightTime = 2,
    }
}

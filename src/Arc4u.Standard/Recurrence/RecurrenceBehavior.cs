using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the behavior of a <see cref="Recurrence"/> regarding invalid times and ambiguous times.
    /// </summary>
    [DataContract]
    public struct RecurrenceBehavior : IEquatable<RecurrenceBehavior>
    {
        private InvalidTimeBehavior _invalidTime;
        /// <summary>
        /// Gets the behavior regarding an invalid time.
        /// An invalid time is a nonexistent time that is an artifact of the transition from standard time to daylight saving time. 
        /// It occurs when the clock time is adjusted forward in time, such as during the transition from a time zone's standard time to its daylight saving time. 
        /// For example, if this transition occurs on a particular day at 2:00 A.M. and causes the time to change to 3:00 A.M., 
        /// each time interval between 2:00 A.M. and 2:59:99 A.M. is invalid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public InvalidTimeBehavior InvalidTime
        {
            get { return _invalidTime; }
            internal set { _invalidTime = value; }
        }

        private AmbiguousTimeBehavior _ambiguousTime;

        /// <summary>
        /// Gets the behavior regarding an ambiguous time.
        /// An ambiguous time is a time that can be mapped to two different times in a single time zone. 
        /// It occurs when the clock time is adjusted back in time, such as during the transition from a time zone's daylight saving time to its standard time. 
        /// For example, if this transition occurs on a particular day at 2:00 A.M. and causes the time to change to 1:00 A.M., 
        /// each time interval between 1:00 A.M. and 1:59:99 A.M. can be interpreted as either a standard time or a daylight saving time.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AmbiguousTimeBehavior AmbiguousTime
        {
            get { return _ambiguousTime; }
            internal set { _ambiguousTime = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrenceBehavior"/> struct 
        /// where invalid times are shift to the daylight saving time
        /// and ambiguous times are considered only on standard time.
        /// </summary>
        /// <returns>A new instance of the <see cref="RecurrenceBehavior"/> struct.</returns>
        public static RecurrenceBehavior ShiftAndStandardTime
        {
            get { return new RecurrenceBehavior(InvalidTimeBehavior.Shift, AmbiguousTimeBehavior.StandardTime); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrenceBehavior"/> struct 
        /// where invalid times are shift to the daylight saving time
        /// and ambiguous times are considered only on daylight saving time.
        /// </summary>
        /// <returns>A new instance of the <see cref="RecurrenceBehavior"/> struct.</returns>
        public static RecurrenceBehavior ShiftAndDaylightTime
        {
            get { return new RecurrenceBehavior(InvalidTimeBehavior.Shift, AmbiguousTimeBehavior.DaylightTime); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrenceBehavior"/> struct 
        /// where invalid times are skipt
        /// and ambiguous times are considered only on standard time.
        /// </summary>
        /// <returns>A new instance of the <see cref="RecurrenceBehavior"/> struct.</returns>
        public static RecurrenceBehavior SkipAndStandardTime
        {
            get { return new RecurrenceBehavior(InvalidTimeBehavior.Skip, AmbiguousTimeBehavior.StandardTime); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrenceBehavior"/> struct 
        /// where invalid times are skipt
        /// and ambiguous times are considered only on daylight saving time.
        /// </summary>
        /// <returns>A new instance of the <see cref="RecurrenceBehavior"/> struct.</returns>
        public static RecurrenceBehavior SkipAndDaylightTime
        {
            get { return new RecurrenceBehavior(InvalidTimeBehavior.Skip, AmbiguousTimeBehavior.DaylightTime); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("InvalidTime {0}, AmbiguousTime {1}"
                , InvalidTime
                , AmbiguousTime);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = 0x4043ed48;
            hash = (hash * -1521134295) + InvalidTime.GetHashCode();
            hash = (hash * -1521134295) + AmbiguousTime.GetHashCode();
            return hash;

        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is RecurrenceBehavior)
                ? Equals((RecurrenceBehavior)obj)
                : false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="RecurrenceBehavior"/>.
        /// </summary>
        /// <param name="other">A <see cref="RecurrenceBehavior"/> to compare to this instance.</param>
        /// <returns>
        ///     <b>true</b> if this instance equals the <paramref name="other"/>; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(RecurrenceBehavior other)
        {
            return object.Equals(InvalidTime, other.InvalidTime)
                && object.Equals(AmbiguousTime, other.AmbiguousTime);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public object Clone()
        {
            return new RecurrenceBehavior(InvalidTime, AmbiguousTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrenceBehavior"/> struct.
        /// </summary>
        /// <param name="invalidTime">The invalid time behavior.</param>
        /// <param name="ambiguousTime">The ambiguous time behavior.</param>
        public RecurrenceBehavior(InvalidTimeBehavior invalidTime, AmbiguousTimeBehavior ambiguousTime)
        {
            _invalidTime = invalidTime;
            _ambiguousTime = ambiguousTime;
        }

        #region Operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left <see cref="RecurrenceBehavior"/>.</param>
        /// <param name="right">The right <see cref="RecurrenceBehavior"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(RecurrenceBehavior left, RecurrenceBehavior right)
        {
            return object.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left <see cref="RecurrenceBehavior"/>.</param>
        /// <param name="right">The right <see cref="RecurrenceBehavior"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(RecurrenceBehavior left, RecurrenceBehavior right)
        {
            return !object.Equals(left, right);
        }

        #endregion
    }
}

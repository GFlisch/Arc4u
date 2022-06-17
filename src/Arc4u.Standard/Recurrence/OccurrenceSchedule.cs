using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents the schedule of an occurrence.
    /// </summary>
    [DataContract]
    public struct OccurrenceSchedule : IEquatable<OccurrenceSchedule>
    {
        [DataMember(Name = "OccursOn", EmitDefaultValue = false)]
        private DateTime occursOn;

        /// <summary>
        /// Gets the scheduled date and time.
        /// </summary>
        public DateTime OccursOn
        {
            get { return occursOn; }
        }

        [DataMember(Name = "DueTime", EmitDefaultValue = false)]
        private TimeSpan dueTime;

        /// <summary>
        /// Gets the scheduled due time.
        /// </summary>
        public TimeSpan DueTime
        {
            get { return dueTime; }
        }

        /// <summary>
        /// Gets the schedule of an immediate occurrence.
        /// </summary>
        public static OccurrenceSchedule Now
        {
            get { return new OccurrenceSchedule(DateTime.UtcNow.AsLocalTime(), TimeSpan.Zero); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Scheduled on {0} in {1}"
                , OccursOn.AsLocalTime().ToString("dd/MM/yyyy HH:mm:ss.fffffff")
                , DueTime);
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
            hash = (hash * -1521134295) + OccursOn.GetHashCode();
            hash = (hash * -1521134295) + DueTime.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<b>true</b> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is OccurrenceSchedule)
                ? Equals((OccurrenceSchedule)obj)
                : false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="OccurrenceSchedule"/>.
        /// </summary>
        /// <param name="other">A <see cref="OccurrenceSchedule"/> to compare to this instance.</param>
        /// <returns>
        ///     <b>true</b> if this instance equals the <paramref name="other"/>; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(OccurrenceSchedule other)
        {
            return object.Equals(OccursOn, other.OccursOn)
                && object.Equals(DueTime, other.DueTime);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public object Clone()
        {
            return new OccurrenceSchedule(OccursOn, DueTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceSchedule"/> struct
        /// where the <see cref="DueTime"/> is calculated from <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="occursOn">The date and time of the occurrence assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        public OccurrenceSchedule(DateTime occursOn)
            : this(occursOn, DateTime.UtcNow.AsLocalTime())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceSchedule"/> struct
        /// where the <see cref="DueTime"/> is calculated from <paramref name="now"/>.
        /// </summary>
        /// <param name="occursOn">The date and time of the occurrence assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        public OccurrenceSchedule(DateTime occursOn, DateTime now)
        {
            //assume universal time to avoid invalid and ambiguous times
            now = now.AsUniversalTime();

            this.occursOn = occursOn;
            this.dueTime = occursOn.SubtractWithDaylightTime(now);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceSchedule"/> struct.
        /// where the <see cref="OccursOn"/> is calculated from <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before reaching the occurrence date and time.</param>
        public OccurrenceSchedule(TimeSpan dueTime)
            : this(dueTime, DateTime.UtcNow.AsLocalTime())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceSchedule"/> struct.
        /// where the <see cref="OccursOn"/> is calculated from <paramref name="now"/>.
        /// </summary>
        /// <param name="dueTime">The amount of time to delay before reaching the occurrence date and time.</param>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        public OccurrenceSchedule(TimeSpan dueTime, DateTime now)
        {
            //assume universal time to avoid invalid and ambiguous times
            now = now.AsUniversalTime();

            this.occursOn = now.Add(dueTime).AsLocalTime();
            this.dueTime = dueTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceSchedule"/> struct.
        /// </summary>
        /// <param name="occursOn">The date and time of the occurrence.</param>
        /// <param name="dueTime">The amount of time to delay before reaching the occurrence date and time.</param>
        internal OccurrenceSchedule(DateTime occursOn, TimeSpan dueTime)
        {
            this.occursOn = occursOn;
            this.dueTime = dueTime;
        }

        #region Operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left <see cref="OccurrenceSchedule"/>.</param>
        /// <param name="right">The right <see cref="OccurrenceSchedule"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(OccurrenceSchedule left, OccurrenceSchedule right)
        {
            return object.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left <see cref="OccurrenceSchedule"/>.</param>
        /// <param name="right">The right <see cref="OccurrenceSchedule"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(OccurrenceSchedule left, OccurrenceSchedule right)
        {
            return !object.Equals(left, right);
        }

        #endregion
    }
}
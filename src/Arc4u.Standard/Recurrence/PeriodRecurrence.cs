using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents a <see cref="Recurrence"/> executed after each elapsed <see cref="Period"/> of time.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [DataContract]
    public sealed class PeriodRecurrence : Recurrence
    {
        #region Properties

        /// <summary>
        /// Gets the recurrence period (ex: every 2 hours).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan Period { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name != null
                ? Name
                : string.Format("Occurs every {0}", Period);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public override object Clone()
        {
            return new PeriodRecurrence(Period);
        }

        /// <summary>
        /// Gets the date and time of the next occurrence from <paramref name="now"/>.
        /// </summary>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>The date and time of the next occurrence in local time.</returns>
        protected override DateTime GetNextTime(DateTime now)
        {
            // consider next time validity
            if (TimeZoneContext.Current.TimeZoneInfo.IsInvalidTime(now.Add(Period))
                && Behavior.InvalidTime == InvalidTimeBehavior.Skip)
            {
                // consider delta between standard time and daylight time
                var delta = TimeZoneContext.Current.GetDaylightChanges(now.Year).Delta;

                // consider period included in the delta
                if (Period < delta)
                {
                    // count how many times the period is included in the delta
                    long count = delta.Ticks / Period.Ticks;

                    // get the next valid time
                    return GetNextTime(now.Add(new TimeSpan(Period.Ticks * count)));
                }

                // get the next valid time
                return GetNextTime(now.Add(Period));
            }

            // consider next time ambiguity
            if (TimeZoneContext.Current.TimeZoneInfo.IsAmbiguousTime(now.Add(Period))
                && Behavior.AmbiguousTime == AmbiguousTimeBehavior.DaylightTime)
            {
                // get the next time from the universal time
                return now.AsUniversalTime().Add(Period).AsLocalTime();
            }

            // get the next time from the local time
            return now.Add(Period);
        }

        #endregion

        #region Constructors

        internal PeriodRecurrence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodRecurrence"/> class 
        /// with the specified <paramref name="period"/> between each occurrence.
        /// </summary>
        /// <param name="period">The time interval between each occurrence.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is not strictly positive.</exception>
        public PeriodRecurrence(TimeSpan period)
            : this(RecurrenceBehavior.ShiftAndStandardTime, period)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodRecurrence"/> class.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="period">The time interval between each occurrence.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is not strictly positive.</exception>
        public PeriodRecurrence(RecurrenceBehavior behavior, TimeSpan period)
            : base(behavior)
        {
            if (period <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("period");

            Period = period;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents a recurrence relying on a calendar.
    /// This is an abstract class.
    /// </summary>
    [DataContract]
    [KnownType(typeof(DailyRecurrence))]
    [KnownType(typeof(WeeklyRecurrence))]
    [KnownType(typeof(MonthlyRecurrence))]
    public abstract class CalendarRecurrence : Recurrence
    {
        #region Properties

        /// <summary>
        /// Gets the recurrent times of day (ex: at 08:00:00, 12:30:00...).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IEnumerable<TimeSpan> TimesOfDay { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRecurrence"/> class.
        /// </summary>
        protected internal CalendarRecurrence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarRecurrence"/> class.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        protected internal CalendarRecurrence(RecurrenceBehavior behavior, IEnumerable<TimeSpan> timesOfDay)
            : base(behavior)
        {
            if (timesOfDay == null || timesOfDay.Count() == 0)
                throw new ArgumentException("timesOfDay");

            var outOfRanges = from timeOfDay in timesOfDay
                              where timeOfDay < TimeSpan.Zero || timeOfDay >= TimeSpan.FromDays(1)
                              select timeOfDay;

            if (outOfRanges.Count() != 0)
                throw new ArgumentOutOfRangeException("timesOfDay", string.Format("Out of range times of day: {0}", string.Join(",", outOfRanges)));

            TimesOfDay = timesOfDay.Distinct().OrderBy(timeOfDay => timeOfDay).ToList();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents a <see cref="Recurrence"/> executed daily 
    /// at the specified <see cref="CalendarRecurrence.TimesOfDay"/>.
    /// This class cannot be inherited.
    /// </summary>
    [DataContract]
    public sealed class DailyRecurrence : CalendarRecurrence
    {
        #region Fields

        private Queue<TimeSpan> timesOfDayQueue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the recurrent day(s) (ex: every two days).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Days { get; internal set; }

        /// <summary>
        /// Gets the recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RecurrentWeekDays WeekDays { get; internal set; }

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
                : Days != 0
                    ? string.Format("Occurs at {0} every {1} day(s)"
                    , string.Join(",", TimesOfDay)
                    , Days)
                    : string.Format("Occurs at {0} on {1}"
                    , string.Join(",", TimesOfDay)
                    , WeekDays);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public override object Clone()
        {
            return (Days != 0)
                ? new DailyRecurrence(Behavior, Days, TimesOfDay)
                : new DailyRecurrence(Behavior, WeekDays, TimesOfDay);
        }

        /// <summary>
        /// Gets the date and time of the next occurrence from <paramref name="now"/>.
        /// </summary>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>The date and time of the next occurrence in local time.</returns>
        protected override DateTime GetNextTime(DateTime inNow)
        {
            if (inNow.Kind == DateTimeKind.Unspecified)
                throw new InvalidTimeZoneException("DateTime Kind should be set. Unspecified is unsupported!");


            var now = inNow.AsLocalTime();

            // consider first occurrence
            var firstOccurrence = (timesOfDayQueue == null);

            // initialize timesOfDayQueue if necessary
            if (timesOfDayQueue == null)
            {
                // consider current dayOfWeek not in requested WeekDays
                if (Days == 0 && (WeekDays & FlagsEnum.PowerOfTwo<RecurrentWeekDays>(now.DayOfWeek)) != FlagsEnum.PowerOfTwo<RecurrentWeekDays>(now.DayOfWeek))
                {
                    timesOfDayQueue = new Queue<TimeSpan>(TimesOfDay);
                }
                else
                {
                    // order the timesOfDay according to the current date and time
                    timesOfDayQueue = new Queue<TimeSpan>(TimesOfDay
                        .SkipWhile(match => match <= now.TimeOfDay)
                        .Concat(TimesOfDay)
                        .Take(TimesOfDay.Count())
                        .DefaultIfEmpty(TimesOfDay.First()));
                }
            }

            // remove the current timeOfDay from the queue
            var timeOfDay = timesOfDayQueue.Dequeue();

            // add the current timeOfDay to the end
            timesOfDayQueue.Enqueue(timeOfDay);

            // get today to maintain the specified timeOfDay across the daylight time
            var today = now.Date;

            // shift today if necessary in order to calculate correctly the next date
            if ((firstOccurrence && now.TimeOfDay < timeOfDay) ||
                (!firstOccurrence && timesOfDayQueue.Count > 1 && timeOfDay > timesOfDayQueue.Min()))
                today = today.AddDays(Days != 0 ? -Days : -1);

            // get the next date
            var nextDate = (Days != 0)
                    ? today.AddDays(Days)
                    : GetNextDateByDayOfWeek(today);

            // consider next time validity
            if (TimeZoneContext.Current.TimeZoneInfo.IsInvalidTime(nextDate.Add(timeOfDay))
                && Behavior.InvalidTime == InvalidTimeBehavior.Skip)
            {
                // get the next valid time
                return GetNextTime(nextDate);
            }

            // consider next time ambiguity
            if (TimeZoneContext.Current.TimeZoneInfo.IsAmbiguousTime(nextDate.Add(timeOfDay))
                && Behavior.AmbiguousTime == AmbiguousTimeBehavior.DaylightTime)
            {
                // get the next time from the universal time
                return nextDate.AsUniversalTime().Add(timeOfDay).AsLocalTime();
            }

            // get the next time from the local time
            return nextDate.Add(timeOfDay);
        }

        /// <summary>
        /// Resets the sequence of occurrences.
        /// </summary>
        protected override void Reset()
        {
            lock (SyncRoot)
            {
                base.Reset();
                timesOfDayQueue = null;
            }
        }

        private DateTime GetNextDateByDayOfWeek(DateTime value)
        {
            var match = default(RecurrentWeekDays);

            do
            {
                value = value.AddDays(1);
                match = FlagsEnum.PowerOfTwo<RecurrentWeekDays>(value.DayOfWeek);
            }
            while ((WeekDays & match) != match);

            return value;
        }

        #endregion

        #region Constructors

        internal DailyRecurrence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class 
        /// executed daily on the specified <paramref name="weekDays"/> 
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="weekDays">The recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="weekDays"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(RecurrentWeekDays weekDays, params TimeSpan[] timesOfDay)
            : this(RecurrenceBehavior.ShiftAndStandardTime, weekDays, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class 
        /// executed daily on the specified <paramref name="weekDays"/> 
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="weekDays">The recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="weekDays"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(RecurrenceBehavior behavior, RecurrentWeekDays weekDays, params TimeSpan[] timesOfDay)
            : this(behavior, weekDays, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class 
        /// executed daily on the specified <paramref name="weekDays"/> 
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="weekDays">The recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="weekDays"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(RecurrenceBehavior behavior, RecurrentWeekDays weekDays, IEnumerable<TimeSpan> timesOfDay)
            : base(behavior, timesOfDay)
        {
            if (weekDays == default(RecurrentWeekDays))
                throw new ArgumentException("Non specified argument.", "weekDays");

            WeekDays = weekDays;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class
        /// executed every <paramref name="days"/>(s)
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="days">The recurrent day(s) (ex: every two days).</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is not strictly positive
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(int days, params TimeSpan[] timesOfDay)
            : this(RecurrenceBehavior.ShiftAndStandardTime, days, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class
        /// executed every <paramref name="days"/>(s)
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="days">The recurrent day(s) (ex: every two days).</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is not strictly positive
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(RecurrenceBehavior behavior, int days, params TimeSpan[] timesOfDay)
            : this(behavior, days, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyRecurrence"/> class
        /// executed every <paramref name="days"/>(s)
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <param name="days">The recurrent day(s) (ex: every two days).</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is not strictly positive
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public DailyRecurrence(RecurrenceBehavior behavior, int days, IEnumerable<TimeSpan> timesOfDay)
            : base(behavior, timesOfDay)
        {
            if (days <= 0)
                throw new ArgumentOutOfRangeException("day");

            Days = days;
        }

        #endregion

    }
}

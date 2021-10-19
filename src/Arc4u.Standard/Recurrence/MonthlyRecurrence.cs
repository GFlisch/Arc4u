using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents a <see cref="Recurrence"/> executed 
    /// every <see cref="DayOfMonth"/> 
    /// or every <see cref="WeekDaysOfMonth"/> 
    /// of specified <see cref="Months"/>
    /// at the specified <see cref="CalendarRecurrence.TimesOfDay"/>.
    /// This class cannot be inherited.
    /// </summary>
    [DataContract]
    public sealed class MonthlyRecurrence : CalendarRecurrence
    {
        #region Fields

        private Queue<TimeSpan> timesOfDayQueue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the recurrent day of month (ex: every 23rd).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int DayOfMonth { get; private set; }

        /// <summary>
        /// Gets the recurrent week days of month (ex: every second Tuesday &amp; Thursday of the month).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RecurrentWeekDaysOfMonth WeekDaysOfMonth { get; private set; }

        /// <summary>
        /// Gets the recurrent months (ex: every January, April).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RecurrentMonths Months { get; private set; }



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
                                : DayOfMonth != 0
                                    ? string.Format("Occurs at {0} every {1} of {2}"
                                        , string.Join(",", TimesOfDay)
                                        , DayOfMonth
                                        , Months)
                                    : string.Format("Occurs at {0} every {1} of {2}"
                                        , string.Join(",", TimesOfDay)
                                        , WeekDaysOfMonth
                                        , Months);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public override object Clone()
        {
            return (DayOfMonth != 0)
                ? new MonthlyRecurrence(Behavior, DayOfMonth, Months, TimesOfDay)
                : new MonthlyRecurrence(Behavior, WeekDaysOfMonth, Months, TimesOfDay);
        }

        /// <summary>
        /// Gets the date and time of the next occurrence from <paramref name="now"/>.
        /// </summary>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>The date and time of the next occurrence.</returns>
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
                if (DayOfMonth == 0 && (WeekDaysOfMonth.WeekDays & FlagsEnum.PowerOfTwo<RecurrentWeekDays>(now.DayOfWeek)) != FlagsEnum.PowerOfTwo<RecurrentWeekDays>(now.DayOfWeek))
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
                today = today.AddDays(-1);

            // get the next date
            var nextDate = (DayOfMonth != 0)
                ? GetNextDateByDayOfMonth(today, Months, DayOfMonth)
                : GetNextDateByWeekDayOfMonth(today, Months, WeekDaysOfMonth);

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

        internal static DateTime GetNextDateByDayOfMonth(DateTime value, RecurrentMonths months, int dayOfMonth)
        {
            var match = default(RecurrentMonths);

            do
            {
                value = value.AddDays(1);
                if (dayOfMonth > DateTime.DaysInMonth(value.Year, value.Month))
                {
                    value = value.AddDays(-value.Day + 1).AddMonths(1).AddDays(-1);
                    continue;
                }

                if (value.Day != dayOfMonth)
                {
                    if (value.Day > dayOfMonth)
                        value = value.AddDays(DateTime.DaysInMonth(value.Year, value.Month) - value.Day).AddDays(dayOfMonth);
                    else
                        value = value.AddDays(dayOfMonth - value.Day);
                }

                match = FlagsEnum.PowerOfTwo<RecurrentMonths>(value.Month - 1);
            }
            while ((months & match) != match || value.Day != dayOfMonth);

            return value;
        }

        internal static DateTime GetNextDateByWeekDayOfMonth(DateTime value, RecurrentMonths months, RecurrentWeekDaysOfMonth weekDayOfMonth)
        {
            var firstOfMonth = value.AddDays(-value.Day + 1);

            //move to next month if necessary when any week days is requested
            if (weekDayOfMonth.AnyWeekDays)
            {
                var positionedDates = from positionedDate in DatesInPosition(firstOfMonth.Year, firstOfMonth.Month, weekDayOfMonth.Position)
                                      let dayOfWeek = FlagsEnum.PowerOfTwo<RecurrentWeekDays>(positionedDate.DayOfWeek)
                                      where (dayOfWeek & weekDayOfMonth.WeekDays) == dayOfWeek
                                      && positionedDate == value
                                      select positionedDate;

                if (positionedDates.Any())
                {
                    firstOfMonth = firstOfMonth.AddMonths(1);
                }
            }

            var nextDate = default(DateTime);
            do
            {
                var positionedDates = from positionedDate in DatesInPosition(firstOfMonth.Year, firstOfMonth.Month, weekDayOfMonth.Position)
                                      let dayOfWeek = FlagsEnum.PowerOfTwo<RecurrentWeekDays>(positionedDate.DayOfWeek)
                                      let month = FlagsEnum.PowerOfTwo<RecurrentMonths>(positionedDate.Month - 1)
                                      where (dayOfWeek & weekDayOfMonth.WeekDays) == dayOfWeek
                                      && (month & months) == month
                                      && positionedDate > value
                                      select positionedDate;

                //move to fourth position if necessary when all weekDays are requested in last position
                if (!weekDayOfMonth.AnyWeekDays
                    && weekDayOfMonth.Position == WeekOfMonthPosition.Last
                    && positionedDates.Count() > 1)
                {
                    //consider positionedDates discontinuity only for continuous weekDays
                    if (FlagsEnum.ContinuousFlagValues<RecurrentWeekDays>(weekDayOfMonth.WeekDays)
                        && positionedDates.First().AddDays(positionedDates.Count() - 1) != positionedDates.Last())
                    {
                        positionedDates = from positionedDate in DatesInPosition(firstOfMonth.Year, firstOfMonth.Month, WeekOfMonthPosition.Fourth)
                                          let dayOfWeek = FlagsEnum.PowerOfTwo<RecurrentWeekDays>(positionedDate.DayOfWeek)
                                          let month = FlagsEnum.PowerOfTwo<RecurrentMonths>(positionedDate.Month - 1)
                                          where (dayOfWeek & weekDayOfMonth.WeekDays) == dayOfWeek
                                          && (month & months) == month
                                          && positionedDate > value
                                          select positionedDate;
                    }
                }

                nextDate = (weekDayOfMonth.AnyWeekDays && weekDayOfMonth.Position == WeekOfMonthPosition.Last)
                    ? positionedDates.LastOrDefault()
                    : positionedDates.FirstOrDefault();
                firstOfMonth = firstOfMonth.AddMonths(1);
            }
            while (nextDate == default(DateTime));

            return nextDate;
        }

        internal static IEnumerable<DateTime> DatesInPosition(int year, int month, WeekOfMonthPosition position)
        {
            var day = default(int);
            var maxDay = default(int);

            if (position != WeekOfMonthPosition.Last)
            {
                day = 1 + 7 * FlagsEnum.PowerOfTwoExponent(position);
                maxDay = day + 6;
            }
            else
            {
                maxDay = DateTime.DaysInMonth(year, month);
                day = maxDay - 6;
            }

            while (day <= maxDay)
            {
                yield return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                day += 1;
            }
        }

        #endregion

        #region Constructors

        internal MonthlyRecurrence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="dayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="dayOfMonth">The recurrent day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dayOfMonth"/> is not between 1 and the greatest days of the specified <paramref name="months"/>
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(int dayOfMonth, RecurrentMonths months, params TimeSpan[] timesOfDay)
            : this(RecurrenceBehavior.ShiftAndStandardTime, dayOfMonth, months, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="dayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="dayOfMonth">The recurrent day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dayOfMonth"/> is not between 1 and the greatest days of the specified <paramref name="months"/>
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(RecurrenceBehavior behavior, int dayOfMonth, RecurrentMonths months, params TimeSpan[] timesOfDay)
            : this(behavior, dayOfMonth, months, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="dayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="dayOfMonth">The recurrent day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dayOfMonth"/> is not between 1 and the greatest days of the specified <paramref name="months"/>
        /// -or- one of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(RecurrenceBehavior behavior, int dayOfMonth, RecurrentMonths months, IEnumerable<TimeSpan> timesOfDay)
            : base(behavior, timesOfDay)
        {
            if (months == default(RecurrentMonths))
                throw new ArgumentException("Non specified argument.", "months");

            //determine upper bound of a leap year
            var upperBound = (from value in FlagsEnum.FlagValues<RecurrentMonths>(months)
                              select DateTime.DaysInMonth(2012, FlagsEnum.PowerOfTwoExponent(value) + 1)).Max();

            if (dayOfMonth < 1 || dayOfMonth > upperBound)
                throw new ArgumentOutOfRangeException("dayOfMonth"
                    , string.Format("Argument is not between 1 and the greatest days {0} of the specified months: {1}."
                    , upperBound
                    , months));

            DayOfMonth = dayOfMonth;
            Months = months;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="weekDayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="weekDayOfMonth">The recurrent week day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="weekDayOfMonth"/> is not specified 
        /// -or- <paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(RecurrentWeekDaysOfMonth weekDayOfMonth, RecurrentMonths months, params TimeSpan[] timesOfDay)
            : this(RecurrenceBehavior.ShiftAndStandardTime, weekDayOfMonth, months, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="weekDayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="weekDayOfMonth">The recurrent week day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="weekDayOfMonth"/> is not specified 
        /// -or- <paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(RecurrenceBehavior behavior, RecurrentWeekDaysOfMonth weekDayOfMonth, RecurrentMonths months, params TimeSpan[] timesOfDay)
            : this(behavior, weekDayOfMonth, months, timesOfDay != null ? timesOfDay.AsEnumerable() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyRecurrence"/> class
        /// executed every <paramref name="weekDayOfMonth"/> of the specified <paramref name="months"/>
        /// at the specified <paramref name="timesOfDay"/>.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <param name="weekDayOfMonth">The recurrent week day of month.</param>
        /// <param name="months">The recurrent months.</param>
        /// <param name="timesOfDay">The times of day the recurrence is executed.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified 
        /// -or- <paramref name="weekDayOfMonth"/> is not specified 
        /// -or- <paramref name="months"/> is not specified 
        /// -or- <paramref name="timesOfDay"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">One of the specified <paramref name="timesOfDay"/> is lower than 00:00:00 or greater than or equal to 1.00:00:00.</exception>
        public MonthlyRecurrence(RecurrenceBehavior behavior, RecurrentWeekDaysOfMonth weekDayOfMonth, RecurrentMonths months, IEnumerable<TimeSpan> timesOfDay)
            : base(behavior, timesOfDay)
        {
            if (weekDayOfMonth == default(RecurrentWeekDaysOfMonth))
                throw new ArgumentException("Non specified argument.", "weekDayOfMonth");

            if (months == default(RecurrentMonths))
                throw new ArgumentException("Non specified argument.", "months");


            WeekDaysOfMonth = weekDayOfMonth;
            Months = months;
        }

        #endregion
    }
}
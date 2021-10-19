using Arc4u.Diagnostics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents a recurrence whose execution depends on its descendants. 
    /// This is an abstract class.
    /// </summary>
    [DataContract]
    //  [KnownType(typeof(PeriodRecurrence))]
    [KnownType(typeof(CalendarRecurrence))]
    public abstract class Recurrence
    {
        #region Fields        

        /// <summary>
        /// Gets an object that can be used to synchronize access.
        /// </summary>
        protected readonly object SyncRoot = new object();

        /// <summary>
        /// Gets or sets the next scheduled time.
        /// </summary>
        protected DateTime NextScheduledTime { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the recurrence. This property is purely informative.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the behavior regarding invalid times and ambiguous times.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RecurrenceBehavior Behavior { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public abstract object Clone();

        /// <summary>
        /// Schedules the specified number of <paramref name="occurrences"/> from <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="occurrences">The number of occurrences to schedule. Specify zero (0) to schedule indefinitely.</param>
        /// <returns>A sequence of <see cref="OccurrenceSchedule"/>.</returns>
        public IEnumerable<OccurrenceSchedule> Schedule(int occurrences)
        {
            return Schedule(occurrences, DateTime.UtcNow.AsLocalTime());
        }

        /// <summary>
        /// Schedules the specified number of <paramref name="occurrences"/> from <paramref name="now"/>.
        /// </summary>
        /// <param name="occurrences">The number of occurrences to schedule. Specify zero (0) to schedule indefinitely.</param>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>A sequence of <see cref="OccurrenceSchedule"/>.</returns>
        public IEnumerable<OccurrenceSchedule> Schedule(int occurrences, DateTime now)
        {
            var occurrence = 0;
            while (occurrences == 0 || occurrence < occurrences)
            {
                var nextSchedule = GetNextSchedule(now);
                yield return nextSchedule;

                now = nextSchedule.OccursOn;
                if (occurrences != -1)
                    occurrence += 1;
            }
        }

        /// <summary>
        /// Gets the schedule of the next occurrence from <see cref="DateTime.Now"/>.
        /// </summary>
        /// <returns>The schedule of the next occurrence.</returns>
        public OccurrenceSchedule GetNextSchedule()
        {
            return GetNextSchedule(DateTime.UtcNow.AsLocalTime());
        }

        /// <summary>
        /// Gets the schedule of the next occurrence from <paramref name="now"/>.
        /// </summary>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>The schedule of the next occurrence.</returns>
        public virtual OccurrenceSchedule GetNextSchedule(DateTime now)
        {
            if (now.Kind == DateTimeKind.Unspecified)
                throw new InvalidTimeZoneException("DateTime Kind should be set. Unspecified is unsupported!");

            // work in local time
            now = now.AsLocalTime();

            var nextTime = default(DateTime);

            lock (SyncRoot)
            {
                // track discontinuous sequence
                if (now < NextScheduledTime)
                {
                    Logger.Technical.From<Recurrence>().Information($"Discontinuous sequence of occurrences between {NextScheduledTime.ToString("o")} and {now.ToString("o")}.").Log();
                    Reset();
                }

                // get the next occurence time
                nextTime = GetNextTime(now);

                // keep next scheduled time
                NextScheduledTime = nextTime;
            }

            // calculate the dueTime
            var dueTime = nextTime.SubtractWithDaylightTime(now);

            return new OccurrenceSchedule(nextTime, dueTime);
        }

        /// <summary>
        /// Gets the date and time of the next occurrence from <paramref name="now"/>.
        /// </summary>
        /// <param name="now">The current date and time assumed to be an universal time if not specified to avoid invalid and ambiguous times.</param>
        /// <returns>The date and time of the next occurrence in local time.</returns>
        protected abstract DateTime GetNextTime(DateTime now);

        /// <summary>
        /// Resets the sequence of occurrences.
        /// </summary>
        protected virtual void Reset()
        {
            lock (SyncRoot)
            {
                NextScheduledTime = default(DateTime).AsLocalTime();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Recurrence"/> class.
        /// </summary>
        protected internal Recurrence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Recurrence"/> class.
        /// </summary>
        /// <param name="behavior">The behavior regarding invalid times and ambiguous times.</param>
        /// <exception cref="ArgumentException"><paramref name="behavior"/> is not specified.</exception>
        protected internal Recurrence(RecurrenceBehavior behavior)
        {
            if (behavior == default(RecurrenceBehavior))
                throw new ArgumentException("Argument non specified.", "behavior");
            Behavior = behavior;
        }

        #endregion
    }
}
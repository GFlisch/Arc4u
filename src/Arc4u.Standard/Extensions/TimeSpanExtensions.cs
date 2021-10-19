using Arc4u;
using System.Diagnostics;

namespace System
{
    /// <summary>
    /// Provides a set of static methods extending the <see cref="TimeSpan"/> class.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Get the due time from <see cref="DateTime.Now"/> to reach a given <see cref="DateTime.TimeOfDay"/>.
        /// </summary>
        /// <param name="timeOfDay">A <see cref="DateTime.TimeOfDay"/>.</param>
        /// <returns>The due time from <see cref="DateTime.Now"/> to reach the specified <paramref name="timeOfDay"/> if this one is between 00:00:00 and 1.00:00:00; otherwise, 
        /// contains negative one (-1) milliseconds if the <paramref name="timeOfDay"/> is greater than 1.00:00:00
        /// or <see cref="TimeSpan.Zero"/> if the <paramref name="timeOfDay"/> is lower than 00:00:00.</returns>        
        public static TimeSpan GetDueTimeOfDay(this TimeSpan timeOfDay)
        {
            return timeOfDay.GetDueTimeOfDay(DateTime.UtcNow.AsLocalTime().TimeOfDay);
        }
        /// <summary>
        /// Get the due time from <paramref name="now"/> to reach a given <see cref="DateTime.TimeOfDay"/>.
        /// </summary>
        /// <param name="timeOfDay">A <see cref="DateTime.TimeOfDay"/>.</param>
        /// <param name="now">A date and time assumed to be a local time.</param>
        /// <returns>The due time from <paramref name="now"/> to reach the specified <paramref name="timeOfDay"/> if this one is between 00:00:00 and 1.00:00:00; otherwise, 
        /// contains negative one (-1) milliseconds if the <paramref name="timeOfDay"/> is greater than 1.00:00:00
        /// or <see cref="TimeSpan.Zero"/> if the <paramref name="timeOfDay"/> is lower than 00:00:00.</returns>    
        public static TimeSpan GetDueTimeOfDay(this TimeSpan timeOfDay, DateTime now)
        {
            //consider no start
            if (timeOfDay > TimeSpan.FromDays(1))
            {
                return TimeSpan.FromMilliseconds(-1);
            }

            //consider immediate start
            if (timeOfDay < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            //correct inexistent dates (eg. 27/03/2011 02:30:00 > 27/03/2011 03:30:30)
            now = now.AsLocalTime();

            //calculate dueTime and associated dueDate
            var dueTime = timeOfDay.GetDueTimeOfDay(now.TimeOfDay);
            var dueDate = now.Add(dueTime);

            //consider daylight saving time for now
            if (TimeZoneContext.Current.TimeZoneInfo.IsDaylightSavingTime(now))
            {
                var delta = TimeZoneContext.Current.GetDaylightChanges(now.Year).Delta;
                dueTime = dueTime.Add(delta);
            }

            //consider daylight saving time for dueDate
            if (TimeZoneContext.Current.TimeZoneInfo.IsDaylightSavingTime(dueDate))
            {
                var delta = TimeZoneContext.Current.GetDaylightChanges(dueDate.Year).Delta;
                dueTime = dueTime.Add(-delta);
            }

            Debug.Assert(dueTime == dueDate.SubtractWithDaylightTime(now));

            return dueTime;
        }

        internal static TimeSpan GetDueTimeOfDay(this TimeSpan timeOfDay, TimeSpan now)
        {
            //consider no start
            if (timeOfDay > TimeSpan.FromDays(1))
            {
                return TimeSpan.FromMilliseconds(-1);
            }

            //consider immediate start
            if (timeOfDay < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            return (timeOfDay >= now)
                ? timeOfDay - now
                : TimeSpan.FromDays(1) - now + timeOfDay;
        }
    }
}
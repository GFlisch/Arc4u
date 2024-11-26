using System.Collections.Generic;
using System.Globalization;
using Arc4u;

namespace System
{
    /// <summary>
    /// Provides a set of static methods extending the <see cref="DateTime"/> class.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the specified <paramref name="value"/> to local time when its <see cref="DateTime.Kind"/> is <see cref="DateTime.Kind.Unspecified"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="DateTime"/> object whose <see cref="DateTime.Kind"/> property is <see cref="DateTime.Kind.Local"/>.</returns>
        public static DateTime AsLocalTime(this DateTime value)
        {
            if (DateTimeKind.Unspecified == value.Kind)
                return DateTime.SpecifyKind(value, DateTimeKind.Local);

            return (value.Kind == DateTimeKind.Utc) ? TimeZoneContext.Current.ConvertFromUtc(value) : value;
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> to Coordinated Universal Time (UTC) when its <see cref="DateTime.Kind"/> is <see cref="DateTime.Kind.Unspecified"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="DateTime"/> object whose <see cref="DateTime.Kind"/> property is <see cref="DateTime.Kind.Utc"/>.</returns>
        public static DateTime AsUniversalTime(this DateTime value)
        {
            if (value.Kind == DateTimeKind.Local)
                return TimeZoneContext.Current.ConvertToUtc(value);

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Subtracts the specified date and time from this <paramref name="instance"/> taking into account the <see cref="DaylightTime"/>.
        /// </summary>
        /// <param name="instance">The <see cref="DateTime"/> instance assumed to be a local time.</param>
        /// <param name="value">The <see cref="DateTime"/> to substract assumed to be a local time.</param>
        /// <returns>A <see cref="TimeSpan"/> interval equal to the date and time represented by this <paramref name="instance"/> minus the date and time represented by <paramref name="value"/>.</returns>
        public static TimeSpan SubtractWithDaylightTime(this DateTime instance, DateTime value)
        {
            var utcDate1 = instance.AsUniversalTime();
            var utcDate2 = value.AsUniversalTime();

            return utcDate1 > utcDate2 ? utcDate1 - utcDate2 : utcDate2 - utcDate1;
        }

        /// <summary>
        /// Converts the time portion to the first hour of the day for the <see cref="DateTime"/> in the desired <see cref="DateTime.Kind"/>.
        /// <see cref="DateTimeKind.Unspecified"/> will be handled the same as <see cref="DateTimeKind.Local"/>. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="kind">A <see cref="DateTime.Kind"/> to specify the desired timezone for the returned <see cref="DateTime"/> object.</param>
        /// <returns>A <see cref="DateTime"/> object with the first hour of the day in the desired <see cref="DateTime.Kind"/>.</returns>
        public static DateTime GetFirstHourOfDay(this DateTime value, DateTimeKind kind)
        {
            value = new DateTime(value.Year, value.Month, value.Day);
            return kind == DateTimeKind.Utc ? value.ToUniversalTime() : value.ToLocalTime();
        }

        /// <summary>
        /// Converts the time portion to the last hour of the day for the <see cref="DateTime"/> in the desired <see cref="DateTime.Kind"/>.
        /// <see cref="DateTimeKind.Unspecified"/> will be handled the same as <see cref="DateTimeKind.Local"/>. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="kind">A <see cref="DateTime.Kind"/> to specify the desired timezone for the returned <see cref="DateTime"/> object.</param>
        /// <returns>A <see cref="DateTime"/> object with the last hour of the day in the desired <see cref="DateTime.Kind"/>.</returns>
        public static DateTime GetLastHourOfDay(this DateTime value, DateTimeKind kind)
        {
            value = new DateTime(value.Year, value.Month, value.Day).AddDays(1).AddHours(-1);
            return kind == DateTimeKind.Utc ? value.ToUniversalTime() : value.ToLocalTime();
        }

        /// <summary>
        /// Converts the time portion to the end of the day for the <see cref="DateTime"/> in the desired <see cref="DateTime.Kind"/>.
        /// <see cref="DateTimeKind.Unspecified"/> will be handled the same as <see cref="DateTimeKind.Local"/>. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="kind">A <see cref="DateTime.Kind"/> to specify the desired timezone for the returned <see cref="DateTime"/> object.</param>
        /// <returns>A <see cref="DateTime"/> object with the last hour of the day in the desired <see cref="DateTime.Kind"/>.</returns>
        public static DateTime GetEndOfDay(this DateTime value, DateTimeKind kind)
        {
            value = new DateTime(value.Year, value.Month, value.Day).AddDays(1);
            return kind == DateTimeKind.Utc ? value.ToUniversalTime() : value.ToLocalTime();
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{DateTime}"/> for every hour between <paramref name="from"/> and <paramref name="target"/> parameters. (both inclusively).
        /// </summary>
        /// <param name="from">A <see cref="DateTime"/> object which represents the start point and the first hourly entry.</param>
        /// <param name="target">A <see cref="DateTime"/> object which marks the target datetime.</param>
        /// <returns>A <see cref="IEnumerable{DateTime}"/> with all hourly values inclusively between <paramref name="from"/> and <paramref name="target"/>. </returns>
        public static IEnumerable<DateTime> EachHour(this DateTime from, DateTime target)
        {
            if (from.Kind != target.Kind)
            {
                target = DateTime.SpecifyKind(target, from.Kind);
            }
            for (var dateTime = from; dateTime <= target; dateTime = dateTime.AddHours(1))
            {
                yield return dateTime;
            }
        }

        /// <summary>
        /// Determines to total number of hours for the provided day in the <paramref name="value"/> parameter.
        /// The total number will change on switch days for the daylight saving.
        /// </summary>
        /// <param name="value">A <see cref="DateTime"/> object for determining the specific day.</param>
        /// <returns>An integer value for the total hours of the specific day provided by <paramref name="value"/>.</returns>
        public static int GetTotalHoursOfDay(this DateTime value)
        {
            var firstHourOfDay = value.GetFirstHourOfDay(DateTimeKind.Utc);
            var endOfDay = firstHourOfDay.GetEndOfDay(DateTimeKind.Utc);

            return Convert.ToInt32((endOfDay - firstHourOfDay).TotalHours);
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> to a <see cref="DateTime"/> without milliseconds. 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="DateTime"/> object of the same <see cref="DateTime.Kind"/> but without milliseconds</returns>
        public static DateTime TruncateMilliseconds(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
        }
    }
}


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
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Local);
            }

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
            {
                return TimeZoneContext.Current.ConvertToUtc(value);
            }

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

    }
}

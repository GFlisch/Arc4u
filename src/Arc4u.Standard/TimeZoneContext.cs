using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace Arc4u.Dependency
{
    // Initialize the TimeZoneContext.
    public static class TimeZoneContextContainerExtension
    {
        public static void InitializeTimeZoneContext(this IServiceProvider app)
        {
            app.GetService<TimeZoneContext>();
        }

    }
}

namespace Arc4u
{
    [Export, Shared]
    public class TimeZoneContext
    {
        internal TimeZoneInfo _timeZone;

        /// <summary>
        /// Determine if the local time is the same as specified in the configuration. If true, convertion to local time can be done simple by setting the kind to local!
        /// </summary>
        //internal bool _isSameTimeZoneInfo;

        public TimeZoneContext(Config config, ILogger logger)
        {
            _timeZone = TimeZoneInfo.Local;
            IntializeFromConfig(config, logger);
            _current = this;
        }

        private static TimeZoneContext _current = null;
        public static TimeZoneContext Current => _current;

        private void IntializeFromConfig(Config config, ILogger logger)
        {
            try
            {
                if (null == config)
                    throw new ArgumentNullException(nameof(config));

                try
                {
                    logger.Technical().From<TimeZoneContext>().System(config.Environment.TimeZone).Log();

                    if (!String.IsNullOrWhiteSpace(config.Environment.TimeZone))
                        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(config.Environment.TimeZone);
                }
                catch (Exception ex)
                {
                    logger.Technical().From<TimeZoneContext>().Warning("Zone not found!").Log();
                    logger.Technical().From<TimeZoneContext>().Exception(ex).Log();
                }

            }
            catch (Exception ex)
            {
                logger.Technical().From<TimeZoneContext>().Exception(ex).Log();
            }
        }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return _timeZone;
            }
        }

#if !WINDOWS_UAP
        public DaylightTime GetDaylightChanges(int inYear)
        {
            TimeZoneInfo.AdjustmentRule ruleFound = null;

            TimeZoneInfo.AdjustmentRule[] adjustments = TimeZoneInfo.GetAdjustmentRules();
            if (adjustments.Length == 0)
            {
                return null; // No Daylighttime.
            }
            //Find the correct adjustment rule
            ruleFound = adjustments.SingleOrDefault(a => a.DateStart.Year <= inYear && a.DateEnd.Year >= inYear);

            if (null == ruleFound) return null;

            DaylightTime outDaylightTime = new DaylightTime(
                                                      GetDateTime(inYear, ruleFound.DaylightTransitionStart)
                                                    , GetDateTime(inYear, ruleFound.DaylightTransitionEnd)
                                                    , ruleFound.DaylightDelta);

            return outDaylightTime;
        }


        private DateTime GetDateTime(int year, TimeZoneInfo.TransitionTime transition)
        {
            // For non-fixed date rules, get local calendar
            Calendar cal = new GregorianCalendar();
            // Get first day of week for transition
            // For example, the 3rd week starts no earlier than the 15th of the month
            int startOfWeek = transition.Week * 7 - 6;
            // What day of the week does the month start on?
            int firstDayOfWeek = (int)cal.GetDayOfWeek(new DateTime(year, transition.Month, 1));
            // Determine how much start date has to be adjusted
            int transitionDay;
            int changeDayOfWeek = (int)transition.DayOfWeek;

            if (firstDayOfWeek <= changeDayOfWeek)
                transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
            else
                transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);

            // Adjust for months with no fifth week
            if (transitionDay > cal.GetDaysInMonth(year, transition.Month))
                transitionDay -= 7;

            return new DateTime(year, transition.Month, transitionDay).AsLocalTime() + transition.TimeOfDay.TimeOfDay;

        }

        private static int GetWeekNumber(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;

            return culture.Calendar.GetWeekOfYear(date,
                culture.DateTimeFormat.CalendarWeekRule,
                culture.DateTimeFormat.FirstDayOfWeek);
        }
#endif

        public DateTime ConvertFromUtc(DateTime value)
        {
            if (DateTimeKind.Utc != value.Kind)
                throw new InvalidTimeZoneException("An Utc date is mandatory!");

            var date = TimeZoneInfo.ConvertTime(value, TimeZoneInfo);
            return DateTime.SpecifyKind(date, DateTimeKind.Local);
        }

        public DateTime ConvertToUtc(DateTime value)
        {
            if (DateTimeKind.Utc == value.Kind)
                return value;

            return TimeZoneInfo.ConvertTime(value, TimeZoneInfo.Utc);
        }
    }
}

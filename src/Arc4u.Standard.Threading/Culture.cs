using System.Globalization;

namespace Arc4u.Threading
{
    /// <summary>
    /// Define the culture for a neutral behavior!
    /// </summary>
    public class Culture
    {
        static Culture()
        {
            _neutral = new CultureInfo("en-GB");
        }

        private static readonly CultureInfo _neutral;

        /// <summary>
        ///  Get the Arc4u neutral culture info!
        /// </summary>
        public static CultureInfo Neutral { get { return _neutral; } }

        /// <summary>
        /// Set the thread to the culture.
        /// </summary>
        /// <param name="culture">The new culture to set.</param>
        public static CultureInfo SetCulture(CultureInfo culture)
        {
            var current = CultureInfo.CurrentCulture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            return current;
        }
    }
}

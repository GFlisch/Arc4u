using System;
using Windows.UI.Xaml.Data;

namespace Arc4u.Windows.Converters
{
    public class ValueWhenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is string && string.IsNullOrEmpty((string)When))
                {
                    if (string.IsNullOrEmpty((string)value))
                        return Value;
                }
                else
                {
                    if (Equals(value, parameter ?? When))
                        return Value;
                }

                return Otherwise ?? value;

            }
            catch
            {
                return Otherwise ?? value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (OtherwiseValueBack == null)
                throw new InvalidOperationException("Cannot ConvertBack if no OtherwiseValueBack is set!");
            try
            {
                if (Equals(value, Value))
                    return When;
                return OtherwiseValueBack;
            }
            catch
            {
                return OtherwiseValueBack;
            }
        }

        public object Value { get; set; }
        public object When { get; set; }
        public object Otherwise { get; set; }
        public object OtherwiseValueBack { get; set; }
    }
}


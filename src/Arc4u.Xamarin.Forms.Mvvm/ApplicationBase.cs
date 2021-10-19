using Xamarin.Forms;

namespace Arc4u.Xamarin.Forms
{
    public abstract class ApplicationBase : Application
    {
        private static bool _disableNavigatingTo;
        public static bool DisableNavigatingTo
        {
            get
            {
                bool tmp = _disableNavigatingTo;
                _disableNavigatingTo = false;
                return tmp;
            }
            set
            {
                _disableNavigatingTo = value;
            }
        }
    }
}

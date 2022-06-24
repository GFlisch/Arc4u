using Arc4u.Threading;
using System;
using System.Globalization;
using System.Threading;

namespace Arc4u.Globalization
{
    /// <summary>
    /// Scopes the <see cref="Thread.CurrentUICulture"/> of the <see cref="Thread.CurrentThread"/>.
    /// </summary>
    public sealed class UICultureScope : Scope<UICultureScope.UICulture>
    {
        public class UICulture
        {
            public CultureInfo Current { get; set; }
        }

        private CultureInfo InitialValue = null;

        private UICultureScope()
        {
        }

        private UICultureScope(CultureInfo culture, bool dispose)
            : base(new UICulture { Current = culture }, false)
        {
            if (null == ParentValue)
                InitialValue = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentUICulture = culture ?? throw new ArgumentNullException("culture");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureScope"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <exception cref="ArgumentNullException"><paramref name="culture"/> is null.</exception>
        public UICultureScope(CultureInfo culture) : this(culture, false)
        {
        }

        /// <summary>
        /// Disposes the ambient scope and its current instance when applicable.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Thread.CurrentThread.CurrentUICulture = null == Current ? InitialValue : Current.Current;
        }
    }
}

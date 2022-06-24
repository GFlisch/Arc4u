using Arc4u.Threading;
using System;
using System.Globalization;
using System.Threading;

namespace Arc4u.Globalization
{

    /// <summary>
    /// Scopes the <see cref="Thread.CurrentCulture"/> of the <see cref="Thread.CurrentThread"/>.
    /// </summary>
    public sealed class CultureScope : Scope<CultureScope.Culture>
    {
        public class Culture
        {
            public CultureInfo Current { get; set; }
        }

        private CultureInfo InitialValue = null;

        private CultureScope()
        {
        }

        private CultureScope(CultureInfo culture, bool dispose)
            : base(new Culture { Current = culture }, false)
        {
            if (null == ParentValue)
                InitialValue = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = culture ?? throw new ArgumentNullException("culture");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureScope"/> class.
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <exception cref="ArgumentNullException"><paramref name="culture"/> is null.</exception>
        public CultureScope(CultureInfo culture) : this(culture, false)
        {
        }

        /// <summary>
        /// Disposes the ambient scope and its current instance when applicable.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Thread.CurrentThread.CurrentCulture = null == Current ? InitialValue : Current.Current;
        }
    }
}

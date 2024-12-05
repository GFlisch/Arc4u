using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Arc4u.Data;

/// <summary>
/// Represents an entity which notifies changes of property value.
/// </summary>
[DataContract]
public abstract class NotifyEntity : INotifyPropertyChanged
{
    private bool _ignoreOnPropertyChanged;

    /// <summary>
    /// Gets a value indicating whether the <see cref="NotifyEntity.PropertyChanged"/> event is ignored or not.
    /// </summary>
    /// <value>
    /// 	<b>true</b> if the <see cref="NotifyEntity.PropertyChanged"/> event is ignored; otherwise, <b>false</b>.
    /// </value>
    protected virtual bool IgnoreOnPropertyChanged
    {
        get { return _ignoreOnPropertyChanged; }
        set { _ignoreOnPropertyChanged = value; }
    }

    /// <summary>
    /// Occurs after a property value changed.
    /// </summary>
    public virtual event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property whose the value has changed.</param>
    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "undefined")
    {
        RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event, 
    /// except when the <see cref="IgnoreOnPropertyChanged"/> property is set to <c>true</c>.
    /// </summary>
    /// <param name="e">A <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
    protected virtual void RaisePropertyChanged(PropertyChangedEventArgs e)
    {
        if (IgnoreOnPropertyChanged)
        {
            return;
        }

        // keep handler to avoid race condition
        var handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "IgnoreOnPropertyChanged = {0}", IgnoreOnPropertyChanged);
    }
}

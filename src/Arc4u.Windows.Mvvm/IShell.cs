using Arc4u.Events;
using System;
using NavigationViewDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode;

namespace Arc4u.Windows
{
    // Any page that will be the main navigation page of the App should implement this method.
    // The frame used by the navigation service, will be based on the frame given.
    public interface IShell
    {
        event EventHandler<ChangedEventArgs<NavigationViewDisplayMode>> DisplayModeChanged;

        event EventHandler<ChangedEventArgs<bool>> IsPaneOpenChanged;

        bool IsPaneOpen { get; set; }

        NavigationViewDisplayMode DisplayMode { get; }

    }
}

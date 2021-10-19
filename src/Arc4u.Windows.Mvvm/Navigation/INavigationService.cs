using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Arc4u.Windows.Navigation
{
    /// <summary>
    /// Provides page based navigation for ViewModels.
    /// </summary>
    public interface INavigationService
    {

        Frame Frame { get; }

        /// <summary>
        /// Navigates to the most recent entry in the back navigation history by popping the calling Page off the navigation stack.
        /// </summary>
        /// <returns>If <c>true</c> a go back operation was successful. If <c>false</c> the go back operation failed.</returns>
        bool GoBack();

        bool CanGoBack { get; }

        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name" />.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        void Navigate<T>();
        void Navigate(Type page);
        void Navigate(String viewName);
        /// <summary>
        /// Initiates navigation to the target specified by the <paramref name="name" />.
        /// </summary>
        /// <param name="name">The name of the target to navigate to.</param>
        /// <param name="parameters">The navigation parameters</param>
        void Navigate<T>(Dictionary<String, Object> parameters);
        void Navigate(Type page, Dictionary<string, object> parameters);
        void Navigate(String viewName, Dictionary<string, object> parameters);
        /// <summary>
        /// Clear the history of calls done on a Frame.
        /// </summary>
        void ClearHistory();

        event EventHandler<NavigationEventArgs> NavigationChanged;
    }
}

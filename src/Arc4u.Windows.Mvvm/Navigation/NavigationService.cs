using Arc4u.Diagnostics;
using Arc4u.Windows.Mvvm;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Arc4u.Windows.Navigation
{
    /// <summary>
    /// This class will manage the navigation from the <see cref="Frame"/> class. 
    /// The main goal of this class is to encapsulate the <see cref="INavigationAware"/> interfaceif implemented
    /// in the View model. If not, there is no advantage to use this class in comparison to work directly with the <see cref="Frame"/> one.
    /// The OnNavigatingTo is not implemented via the event OnNavigating because any async call to a Window popup, like a <see  cref="MessageDialog"/> will
    /// simply not wait but continue the navigation process (NavigateFrom and NavigateTo)!
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;

        public event EventHandler<NavigationEventArgs> NavigationChanged;

        public NavigationService() : this(new Frame())
        {
        }

        public NavigationService(Frame frame)
        {
            _frame = frame;
            _frame.Navigated += _frame_Navigated;
        }

        private Page CurrentPage { get; set; }
        private async void _frame_Navigated(object sender, global::Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (NavigationMode.New == e.NavigationMode)
            {
                try
                {
                    if (null != CurrentPage)
                    {
                        if (CurrentPage.DataContext is INavigationAware navigationAware)
                            await navigationAware.OnNavigatedFromAsync();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Technical.From<NavigationService>().Exception(ex).Log();
                }

                try
                {
                    if (e.Content is Page page && page.DataContext is INavigationAware navigationAware)
                    {
                        CurrentPage = page;
                        await navigationAware.OnNavigatedToAsync(e.Parameter as Dictionary<string, object>);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Technical.From<NavigationService>().Exception(ex).Log();
                }

            }

            if (NavigationMode.Back == e.NavigationMode && null != CurrentPage) // shoul be always true!
            {
                try
                {
                    if (CurrentPage.DataContext is INavigationAware navigationAware)
                        await navigationAware.OnNavigatedFromAsync();
                }
                catch (Exception ex)
                {
                    Logger.Technical.From<NavigationService>().Exception(ex).Log();
                }

                try
                {
                    if (e.Content is Page page && page.DataContext is INavigationAware navigationAware)
                    {
                        CurrentPage = page;
                        await navigationAware.OnNavigatedToAsync(new Dictionary<string, object>() { { "Back", true } });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Technical.From<NavigationService>().Exception(ex).Log();
                }

            }


            NavigationChanged?.Invoke(sender, e);

        }

        public Frame Frame => _frame;

        internal static Func<String, Type> ResolveViewFromName { get; private set; }

        public static void SetResolveViewFromName(Func<String, Type> resolver)
        {
            ResolveViewFromName = resolver;
        }


        public virtual void ClearHistory()
        {
            _frame.BackStack.Clear();
        }

        public bool GoBack()
        {
            if (CanGoBack)
            {
                if (null != CurrentPage)
                {
                    try
                    {
                        if (CurrentPage.DataContext is INavigationAware navigationAware)
                        {
                            DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                            {
                                if (await navigationAware.OnNavigatingFromAsync(true))
                                    _frame.GoBack();
                            });
                        }


                    }
                    catch (Exception ex)
                    {
                        Logger.Technical.From<NavigationService>().Exception(ex).Log();
                    }
                }
                return true;
            }

            return false;
        }

        public bool CanGoBack => _frame.CanGoBack;
        public void Navigate<T>()
        {
            Navigate<T>(new Dictionary<string, object>());
        }

        public void Navigate(Type page)
        {
            Navigate(page, new Dictionary<string, object>());
        }
        public void Navigate(String viewName)
        {
            Navigate(ResolveViewFromName(viewName), new Dictionary<string, object>());
        }

        public void Navigate(Type page, Dictionary<string, object> parameters)
        {
            DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                if (null != CurrentPage)
                {
                    try
                    {
                        if (CurrentPage.DataContext is INavigationAware navigationAware)
                        {
                            if (await navigationAware.OnNavigatingFromAsync(false))
                                _frame.Navigate(page, parameters);
                        }


                    }
                    catch (Exception ex)
                    {
                        Logger.Technical.From<NavigationService>().Exception(ex).Log();
                    }
                }
                else
                    _frame.Navigate(page, parameters);
            });
        }

        public void Navigate<T>(Dictionary<string, object> parameters)
        {

            Navigate(typeof(T), parameters);
        }

        public void Navigate(String viewName, Dictionary<string, object> parameters)
        {
            Navigate(ResolveViewFromName(viewName), parameters);
        }
    }
}

using Arc4u.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Arc4u.Windows.Mvvm
{
    public static class ViewModelLocator
    {
        public static bool GetAutowireViewModel(Page obj)
        {
            return (bool)obj.GetValue(AutowireViewModelProperty);
        }
        public static void SetAutowireViewModel(Page obj, bool value)
        {
            obj.SetValue(AutowireViewModelProperty, value);
        }
        public static readonly DependencyProperty AutowireViewModelProperty =
                DependencyProperty.RegisterAttached("AutowireViewModel", typeof(bool),
                        typeof(ViewModelLocator),
                        new PropertyMetadata(null, AutowireViewModelChanged));

        private static void AutowireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!global::Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                try
                {
                    if (((bool)e.NewValue) == true)
                    {
                        ViewModelLocationProvider.AutoWireViewModelChanged(d, Bind);
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Technical.From(typeof(ViewModelLocator)).Exception(ex).Log();
                }

            }
        }
        private static void Bind(object view, object viewModel)
        {
            try
            {
                (view as Page).DataContext = viewModel;

            }
            catch (System.Exception ex)
            {
                Logger.Technical.From(typeof(ViewModelLocator)).Exception(ex).Log();
            }
        }
    }
}


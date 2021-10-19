using System;

namespace Arc4u.Xamarin.Forms.Mvvm
{
    public static class ViewModelLocationProvider
    {
        /// <summary>
        /// The default view model factory which provides the ViewModel type as a parameter.
        /// </summary>
        static Func<Type, object> _defaultViewModelFactory;

        /// <summary>
        /// Default view type to view model type resolver, assumes the view model is in same assembly as the view type, but in the "ViewModels" namespace.
        /// </summary>
        static Func<Type, Type> _defaultViewTypeToViewModelTypeResolver =
            viewType =>
            {
                var viewName = viewType.FullName;
                viewName = viewName.Replace(".Views.", ".ViewModels.");
                var viewAssemblyName = viewType.Assembly.FullName;
                var viewModelName = $"{viewName}ViewModel, {viewAssemblyName}";
                var t = Type.GetType(viewModelName);
                return t;
            };

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory which provides the ViewModel type as a parameter.</param>
        public static void SetDefaultViewModelFactory(Func<Type, object> viewModelFactory)
        {
            _defaultViewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view type to view model type resolver.
        /// </summary>
        /// <param name="viewTypeToViewModelTypeResolver">The view type to view model type resolver.</param>
        public static void SetDefaultViewTypeToViewModelTypeResolver(Func<Type, Type> viewTypeToViewModelTypeResolver)
        {
            _defaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }

        /// <summary>
        /// Automatically looks up the viewmodel that corresponds to the current view, using two strategies:
        /// It first looks to see if there is a mapping registered for that view, if not it will fallback to the convention based approach.
        /// </summary>
        /// <param name="view">The dependency object, typically a view.</param>
        /// <param name="setDataContextCallback">The call back to use to create the binding between the View and ViewModel</param>
        public static void AutoWireViewModelChanged(object view, Action<object, object> setDataContextCallback)
        {
            // Try mappings first
            object viewModel = null;

            //check type mappings
            var viewModelType = _defaultViewTypeToViewModelTypeResolver(view.GetType());

            if (viewModelType == null)
                return;

            viewModel = _defaultViewModelFactory(viewModelType);

            setDataContextCallback(view, viewModel);
        }
    }
}
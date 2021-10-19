using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Arc4u.Xamarin.Forms.Mvvm
{
    public class ShellNavigationBehavior : Behavior<Shell>
    {
        public Shell AssociatedPage { get; set; }

        protected override void OnAttachedTo(Shell bindable)
        {
            base.OnAttachedTo(bindable);

            AssociatedPage = bindable;


            AssociatedPage.Navigating += AssociatedPage_Navigating;
        }

        private async void AssociatedPage_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            var vm = AssociatedPage.BindingContext;
            if (vm is INavigationAware)
            {
                await ((INavigationAware)vm).OnNavigatingToAsync(new Dictionary<string, Object>());
            }
        }

        protected override void OnDetachingFrom(Shell bindable)
        {
            base.OnDetachingFrom(bindable);
            AssociatedPage.Navigating -= AssociatedPage_Navigating;

            AssociatedPage = null;
        }
    }
}
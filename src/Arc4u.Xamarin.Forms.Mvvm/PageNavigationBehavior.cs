using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Arc4u.Xamarin.Forms.Mvvm
{
    public class PageNavigationBehavior : Behavior<ShellContentPage>
    {
        public ShellContentPage AssociatedPage { get; set; }

        protected override void OnAttachedTo(ShellContentPage bindable)
        {
            base.OnAttachedTo(bindable);

            AssociatedPage = bindable;

            AssociatedPage.Appearing += AssociatedPage_Appearing;
            AssociatedPage.Disappearing += AssociatedPage_Disappearing;
        }



        protected override void OnDetachingFrom(ShellContentPage bindable)
        {
            base.OnDetachingFrom(bindable);

            AssociatedPage.Appearing -= AssociatedPage_Appearing;
            AssociatedPage.Disappearing -= AssociatedPage_Disappearing;

            AssociatedPage = null;
        }

        private async void AssociatedPage_Appearing(object sender, EventArgs e)
        {
            if (!ApplicationBase.DisableNavigatingTo)
            {
                var vm = AssociatedPage.BindingContext;
                if (vm is INavigationAware)
                {
                    await ((INavigationAware)vm).OnNavigatingToAsync(new Dictionary<string, object>(AssociatedPage.Parameters));
                }
            }
        }

        private async void AssociatedPage_Disappearing(object sender, EventArgs e)
        {
            var vm = AssociatedPage.BindingContext;
            if (vm is INavigationAware)
            {
                await ((INavigationAware)vm).OnNavigatingFromAsync();
                // With shell the GotoAsync doesn't create a new page if it exists!
                // We erase the Parameters on the page to avoid conflicts.
                AssociatedPage.Parameters = new Dictionary<string, Object>();
            }
        }
    }
}

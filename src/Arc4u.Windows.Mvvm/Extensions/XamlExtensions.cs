using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Arc4u.Windows.Extension
{
    public static class XamlExtensions
    {
        public static T FirstAncestor<T>(this DependencyObject control) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(control);
            while (parent != null)
            {
                if (parent is T) return (T)parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        public static IList<DependencyObject> AllAncestors(this DependencyObject control)
        {
            var list = new List<DependencyObject>();
            var parent = VisualTreeHelper.GetParent(control);
            if (parent != null)
            {
                list.Add(parent);
                list.AddRange(AllAncestors(parent));
            }
            return list;
        }

        public static T FirstChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            return parent.AllChildren().Where(e => e is T).FirstOrDefault() as T;
        }

        public static List<DependencyObject> AllChildren(this DependencyObject parent)
        {
            var list = new List<DependencyObject>();
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                list.Add(child);
                list.AddRange(AllChildren(child));
            }
            return list;
        }

        public static ElementTheme ToElementTheme(this ApplicationTheme theme)
        {
            switch (theme)
            {
                case ApplicationTheme.Light:
                    return ElementTheme.Light;
                case ApplicationTheme.Dark:
                default:
                    return ElementTheme.Dark;
            }
        }

        public static ApplicationTheme ToApplicationTheme(this ElementTheme theme)
        {
            switch (theme)
            {
                case ElementTheme.Default:
                    return ApplicationTheme.Dark;
                case ElementTheme.Light:
                    return ApplicationTheme.Light;
                case ElementTheme.Dark:
                default:
                    return ApplicationTheme.Dark;
            }
        }

        public static void SetAsNotSet(this DependencyObject o, DependencyProperty dp)
        {
            o.SetValue(dp, DependencyProperty.UnsetValue);
        }

        public static void SetIfNotSet(this DependencyObject o, DependencyProperty dp, object value)
        {
            if (o.ReadLocalValue(dp) == DependencyProperty.UnsetValue)
                o.SetValue(dp, value);
        }

    }
}

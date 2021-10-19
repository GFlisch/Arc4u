using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace Arc4u.Windows.Behaviors
{
    public abstract class Behavior<T> : DependencyObject, IBehavior where T : DependencyObject
    {
        public DependencyObject AssociatedObject => _associatedObject;


        private T _associatedObject;


        public void Attach(DependencyObject associatedObject)
        {
            _associatedObject = associatedObject as T;
            OnAttach(_associatedObject);
        }

        public void Detach()
        {
            OnDetach(_associatedObject);
            _associatedObject = null;
        }

        protected virtual void OnAttach(T associatedObject)
        {
        }

        protected virtual void OnDetach(T associatedObject)
        {

        }
    }
}

using Arc4u.Windows.Navigation;
using Windows.UI.Xaml.Controls;

namespace Arc4u.Windows.Controls
{
    public class RegionFrame : Frame
    {
        public RegionFrame() : base()
        {
            this.Loaded += RegionFrame_Loaded;
        }

        // Register when the frame is loaded. So the name of the frame is available!
        private void RegionFrame_Loaded(object sender, global::Windows.UI.Xaml.RoutedEventArgs e)
        {
            Shell.Regions.Add(this);
        }
    }
}

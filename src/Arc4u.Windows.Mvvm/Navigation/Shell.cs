namespace Arc4u.Windows.Navigation
{
    public static class Shell
    {
        static Shell()
        {
            Navigation = new NavigationService();
            Regions = new RegionManager();
        }

        public static RegionManager Regions { get; set; }

        public static INavigationService Navigation { get; private set; }


    }
}

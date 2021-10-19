using Arc4u.Diagnostics;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Arc4u.Windows.Navigation
{
    public class RegionManager
    {
        public RegionManager()
        {
            Navigations = new Dictionary<string, INavigationService>();
        }

        internal void Add(Frame region)
        {
            if (String.IsNullOrWhiteSpace(region.Name))
                throw new ArgumentNullException("Name");

            if (Navigations.ContainsKey(region.Name))
            {
                Logger.Technical.From<RegionManager>().System($"A region with name: {region.Name} is already registered.").Log();
                throw new DuplcateRegionException(region.Name);
            }

            Navigations.Add(region.Name, new NavigationService(region));
        }

        public void Clear()
        {
            Navigations.Clear();
        }

        private Dictionary<String, INavigationService> Navigations { get; set; }


        public INavigationService this[string region]
        {
            get
            {
                if (!Navigations.ContainsKey(region))
                    throw new RegionNotFoundException(region);

                return Navigations[region];
            }
        }

    }
}

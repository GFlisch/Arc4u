using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Xamarin.Essentials;

namespace Arc4u.Network.Connectivity
{
    [Export(typeof(INetworkInformation)), Shared]
    public class NetworkInformation : INetworkInformation
    {
        public NetworkInformation()
        {
            Xamarin.Essentials.Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            CurrentStatus = GetNetworkStatus(Xamarin.Essentials.Connectivity.NetworkAccess, Xamarin.Essentials.Connectivity.ConnectionProfiles);
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var networkInfo = GetNetworkStatus(e.NetworkAccess, e.ConnectionProfiles);

            if (CurrentStatus != networkInfo)
            {
                StatusMonitoring?.Invoke(this, new NetworkInformationArgs(networkInfo));
                CurrentStatus = networkInfo;
            }
        }

        NetworkStatus CurrentStatus = NetworkStatus.None;

        private NetworkStatus GetNetworkStatus(Xamarin.Essentials.NetworkAccess status, IEnumerable<Xamarin.Essentials.ConnectionProfile> profiles)
        {
            NetworkStatus networkInfo = 0;

            switch (status)
            {
                case Xamarin.Essentials.NetworkAccess.None:
                    return NetworkStatus.None;
                case Xamarin.Essentials.NetworkAccess.Internet:
                    networkInfo = NetworkStatus.Internet;
                    break;
                case Xamarin.Essentials.NetworkAccess.ConstrainedInternet:
                case Xamarin.Essentials.NetworkAccess.Local:
                    networkInfo = NetworkStatus.Local;
                    break;
                default:
                    return NetworkStatus.None;
            }

            // Add more info in case we have a connection.

            if (profiles.Any(p => p == Xamarin.Essentials.ConnectionProfile.WiFi))
                networkInfo |= NetworkStatus.Wifi;
            if (profiles.Any(p => p == Xamarin.Essentials.ConnectionProfile.Ethernet))
                networkInfo |= NetworkStatus.Ethernet;
            if (profiles.Any(p => p == Xamarin.Essentials.ConnectionProfile.Bluetooth))
                networkInfo |= NetworkStatus.Bluetooth;
            if (profiles.Any(p => p == Xamarin.Essentials.ConnectionProfile.Cellular))
                networkInfo |= NetworkStatus.Cellular;

            return networkInfo;
        }

        public NetworkStatus Status => GetNetworkStatus(Xamarin.Essentials.Connectivity.NetworkAccess, Xamarin.Essentials.Connectivity.ConnectionProfiles);


        public event EventHandler<NetworkInformationArgs> StatusMonitoring;
    }
}

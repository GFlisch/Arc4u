using System;
using System.Composition;
using System.Linq;
using Windows.Networking.Connectivity;

namespace Arc4u.Network.Connectivity
{
    [Export(typeof(INetworkInformation)), Shared]
    public class NetworkInformation : INetworkInformation
    {
        public NetworkInformation()
        {
            global::Windows.Networking.Connectivity.NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            var handler = StatusMonitoring;
            if (null != handler)
            {
                handler.Invoke(this, new NetworkInformationArgs(GetStatus()));
            }
        }

        public NetworkStatus Status => GetStatus();

        private static NetworkStatus GetStatus()
        {
            var connectionProfile = global::Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();

            if (null == connectionProfile) return NetworkStatus.None;

            NetworkStatus networkInfo = 0;

            var status = connectionProfile.GetNetworkConnectivityLevel();

            switch (status)
            {
                case NetworkConnectivityLevel.None:
                    return NetworkStatus.None;
                case NetworkConnectivityLevel.InternetAccess:
                    networkInfo = NetworkStatus.Internet;
                    break;
                case NetworkConnectivityLevel.ConstrainedInternetAccess:
                case NetworkConnectivityLevel.LocalAccess:
                    networkInfo = NetworkStatus.Local;
                    break;
                default:
                    return NetworkStatus.None;
            }

            // Add more info in case we have a connection.

            var networkInterfaceList = global::Windows.Networking.Connectivity.NetworkInformation.GetConnectionProfiles();

            foreach (var interfaceInfo in networkInterfaceList.Where(nii => nii.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None))
            {
                if (interfaceInfo.NetworkAdapter != null)
                {
                    // http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
                    switch (interfaceInfo.NetworkAdapter.IanaInterfaceType)
                    {
                        case 6:
                            networkInfo |= NetworkStatus.Ethernet;
                            break;
                        case 71:
                            networkInfo |= NetworkStatus.Wifi;
                            break;
                        case 243:
                        case 244:
                            networkInfo |= NetworkStatus.Cellular;
                            break;
                        case 281:
                            continue;
                    }
                }

            }


            return networkInfo;

        }

        public event EventHandler<NetworkInformationArgs> StatusMonitoring;
    }
}

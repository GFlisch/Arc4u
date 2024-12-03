namespace Arc4u.Network.Connectivity;

public interface INetworkInformation
{
    NetworkStatus Status { get; }

    event EventHandler<NetworkInformationArgs> StatusMonitoring;
}

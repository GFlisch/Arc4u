namespace Arc4u.Network.Connectivity;

public class NetworkInformationArgs : EventArgs
{
    public NetworkInformationArgs(NetworkStatus newStatus)
    {
        NewStatus = newStatus;
    }

    public NetworkStatus NewStatus { get; }
}

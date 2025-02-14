using Arc4u.Dependency.Attribute;

namespace Arc4u.Network.Connectivity;

[Export(typeof(INetworkInformation))]
public class AlwaysConnected : INetworkInformation
{
    public AlwaysConnected()
    {
        // Suppress a warning (in fact will be actually not usefull)
        var handler = StatusMonitoring;
        handler?.Invoke(this, new NetworkInformationArgs(Status));
    }

    public NetworkStatus Status => NetworkStatus.Ethernet | NetworkStatus.Internet;

    public event EventHandler<NetworkInformationArgs>? StatusMonitoring;
}

namespace Arc4u.Network.Connectivity;

[Flags]
public enum NetworkStatus
{
    None = 1,

    Local = 2,

    Internet = 4,

    Cellular = 8,

    Wifi = 16,

    Ethernet = 32,

    Bluetooth = 64,
}

using System.Text;
using Arc4u.Network.Connectivity;

namespace Arc4u.Exceptions
{
    public class NetworkException : Exception
    {
        public NetworkException(NetworkStatus networkStatus) : base()
        {
            NetworkStatus = networkStatus;
        }

        public NetworkException(NetworkStatus networkStatus, string message) : base(message)
        {
            NetworkStatus = networkStatus;
        }

        public NetworkStatus NetworkStatus { get; set; }

        public override string ToString()
        {
            var value = new StringBuilder();
            value.AppendLine($"Network connectivity is equal to {NetworkStatus}.");
            value.AppendLine(base.ToString());

            return value.ToString();
        }

    }
}

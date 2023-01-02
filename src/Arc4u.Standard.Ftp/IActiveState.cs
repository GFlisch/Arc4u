using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Arc4u.Standard.UnitTest")]

namespace Arc4u.Standard.Ftp;

internal interface IActiveState
{
    bool IsConnected { get; }
}
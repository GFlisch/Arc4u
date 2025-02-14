using System;

namespace Arc4u.Diagnostics
{
    /// <summary>
    /// The purpose of the interface is to implement the creation of the message source following the
    /// platform used.
    /// </summary>
    [Obsolete("Use Serilog")]
    public interface IMessageSourceFactory
    {
        MessageSource Create(String application, int eventId);

        MessageSource Create(String stackTrace, String application, int eventId);
    }
}

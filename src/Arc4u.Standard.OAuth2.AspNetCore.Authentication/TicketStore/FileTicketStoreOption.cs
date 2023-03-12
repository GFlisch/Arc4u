using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.TicketStore;

public class FileTicketStoreOption
{
    public DirectoryInfo? StorePath { get; set; }

    public string? TicketStore { get; set; } = typeof(FileTicketStore).FullName;
}

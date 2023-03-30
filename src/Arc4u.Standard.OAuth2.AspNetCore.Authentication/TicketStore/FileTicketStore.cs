using System;
using System.IO;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TicketStore;

public class FileTicketStore : ITicketStore
{
    public FileTicketStore(ILogger<FileTicketStore> logger, IOptionsMonitor<FileTicketStoreOptions> options)
    {
        _logger = logger;

        ArgumentNullException.ThrowIfNull(options.CurrentValue.StorePath, nameof(options.CurrentValue.StorePath));

        _directoryStore = options.CurrentValue.StorePath;
    }

    private readonly DirectoryInfo _directoryStore;
    private readonly object _lock = new();
    private readonly ILogger<FileTicketStore> _logger;
    public Task RemoveAsync(string key)
    {
        var fullPath = GetPath(key);

        lock (_lock)
        {
            File.Delete(fullPath);
            _logger.Technical().LogDebug($"Remove ticket with key: {key}");
        }

        return Task.CompletedTask;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await RemoveAsync(key).ConfigureAwait(false);
        var fullPath = GetPath(key);

        lock (_lock)
        {
            File.WriteAllBytes(fullPath, TicketSerializer.Default.Serialize(ticket));
            _logger.Technical().LogDebug($"Renew ticket with key: {key} on path: {fullPath}");
        }

        return;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var fullPath = GetPath(key);

        AuthenticationTicket? ticket = null;

        if (File.Exists(fullPath))
        {
            var content = File.ReadAllBytes(fullPath);

            ticket = TicketSerializer.Default.Deserialize(content);
            _logger.Technical().LogDebug($"Get ticket with key: {key} on path: {fullPath}");
        }
        else
        {
            _logger.Technical().LogDebug($"Get ticket with key: {key} on path: {fullPath}");
        }

        return Task.FromResult(ticket);
    }

    private string GetPath(string key) => Path.Combine(_directoryStore.FullName, key + ".bin");

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString();
        var fullPath = GetPath(key);

        lock (_lock)
        {
            File.WriteAllBytes(fullPath, TicketSerializer.Default.Serialize(ticket));
            _logger.Technical().LogDebug($"Create ticket with key: {key} on path: {fullPath}");
        }

        return Task.FromResult(key);
    }
}

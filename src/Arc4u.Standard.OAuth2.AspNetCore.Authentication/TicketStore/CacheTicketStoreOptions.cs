namespace Arc4u.OAuth2.TicketStore;

public class CacheTicketStoreOptions
{
    public string CacheName { get; set; } = "Default";

    public string KeyPrefix { get; set; } = "AuthSessionStore-";

    public string? TicketStore { get; set; } = typeof(CacheTicketStore).FullName;
}


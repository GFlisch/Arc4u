namespace Arc4u.OAuth2.TicketStore;

public class CacheTicketStoreOption
{
    public string CacheName { get; set; } = "Default";

    public string KeyPrefix { get; set; } = "AuthSessionStore-";

    public string? TicketStore { get; set; } = typeof(CacheTicketStore).FullName;
}


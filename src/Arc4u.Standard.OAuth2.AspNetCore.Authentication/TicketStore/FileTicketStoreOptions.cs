namespace Arc4u.OAuth2.TicketStore;

public class FileTicketStoreOptions
{
    public DirectoryInfo? StorePath { get; set; }

    public string? TicketStore { get; set; } = typeof(FileTicketStore).FullName;
}

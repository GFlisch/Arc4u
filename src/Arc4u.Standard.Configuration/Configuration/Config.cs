namespace Arc4u.Configuration;

public class ApplicationConfig
{
    /// <summary>
    /// Name used to identify the application when used externally other than logging!
    /// Cache or authorization, etc...
    /// </summary>
    public string ApplicationName { get; set; }

    public Environment Environment { get; set; } = new Environment();
}

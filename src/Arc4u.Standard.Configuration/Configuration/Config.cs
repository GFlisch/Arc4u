using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration;

public class ApplicationConfig
{
    public ApplicationConfig() => Environment = new Environment();

     /// <summary>
    /// Name used to dentify the application when used externally other than logging!
    /// Cache or authorization, etc...
    /// </summary>
    public string ApplicationName { get; set; }

    public Environment Environment { get; set; } = new Environment();
}

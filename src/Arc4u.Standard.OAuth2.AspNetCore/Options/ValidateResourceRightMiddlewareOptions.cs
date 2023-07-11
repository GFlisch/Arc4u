namespace Arc4u.OAuth2.AspNetCore.Options;
public class ValidateResourceRightMiddlewareOptions
{
    public string AuthorizationPolicy { get; set; }

    public string Path { get; set; }

    /// <summary>
    /// Overide the default content if specified.
    /// </summary>
    public string ContentToDisplay { get; set; } = string.Empty;

}

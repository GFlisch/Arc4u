namespace Arc4u.Standard.OAuth2.Middleware
{
    public class OpenIdBearerInjectorOptions
    {
        public IKeyValueSettings OpenIdSettings { get; set; } = null;

        public IKeyValueSettings OAuth2Settings { get; set; } = null;
    }
}

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class OpenIdBearerInjectorOptions
    {
        public IKeyValueSettings OpenIdSettings { get; set; } = null;

        public IKeyValueSettings OAuth2Settings { get; set; } = null;

        /// <summary>
        /// Which provider is used to create an On behal of token.
        /// </summary>
        public string OboProviderKey { get; set; } = "Obo";

        /// <summary>
        /// The OpenId KeyValues settings resolver name
        /// </summary>
        public string OpenIdProviderKey { get; set; } = "OpenId";
    }
}

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class OpenIdBearerInjectorOptions
    {
        public string OnBehalfOfOpenIdSettingsKey { get; set; } = "Obo_for_OpenId";

        /// <summary>
        /// Which provider is used to create an On behal of token.
        /// </summary>
        public string OboProviderKey { get; set; } = "Obo";

        /// <summary>
        /// The OpenId KeyValues settings resolver name
        /// </summary>
        public string OpenIdSettingsKey { get; set; } = "OpenId";
    }

    public class OpenIdBearerInjectorSettingsOptions
    {
        public IKeyValueSettings OnBehalfOfOpenIdSettings { get; set; }

        /// <summary>
        /// Which provider is used to create an On behal of token.
        /// </summary>
        public string OboProviderKey { get; set; } = "Obo";

        /// <summary>
        /// The OpenId KeyValues settings resolver name
        /// </summary>
        public IKeyValueSettings OpenIdSettings { get; set; }
    }
}

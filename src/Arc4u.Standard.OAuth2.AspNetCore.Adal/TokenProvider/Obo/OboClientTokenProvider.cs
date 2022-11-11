using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider)), Shared]
    public class OboClientTokenProvider : ITokenProvider
    {
        public const string ProviderName = "oboClient";

        private readonly Dictionary<int, IKeyValueSettings> _settings;
        private readonly Dictionary<string, TokenInfo> _tokenInfos;
        private readonly ILogger<OboClientTokenProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public OboClientTokenProvider(ILogger<OboClientTokenProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = new Dictionary<int, IKeyValueSettings>();
            _tokenInfos = new Dictionary<string, TokenInfo>();
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {

            if (!_settings.ContainsKey(settings.GetHashCode()))
                ValidateSettings(settings);

            if (_tokenInfos.TryGetValue(settings.Values["OboEndpointUrl"], out var tokenInfo))
            {
                if (tokenInfo.ExpiresOnUtc > DateTime.UtcNow.AddMinutes(-1))
                    return tokenInfo;
            }

            // tokenInfo is expired or not yet available!
            var oauth = settings.Values.ContainsKey("OAuthSettingsReader") ? settings.Values["OAuthSettingsReader"] : "OAuth";
            var httpClient = _httpClientFactory.CreateClient(oauth);

            try
            {
                var response = await httpClient.GetAsync(settings.Values["OboEndpointUrl"]);
                if (response.IsSuccessStatusCode)
                {
                    var accessToken = await response.Content.ReadAsStringAsync();
                    // validate that is a beare token!
                    var jwtToken = new JwtSecurityToken(accessToken);

                    tokenInfo = new TokenInfo("Bearer", accessToken, jwtToken.ValidTo);

                    _tokenInfos[settings.Values["OboEndpointUrl"]] = tokenInfo;

                    return tokenInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();

                throw ex;
            }

            throw new ApplicationException("No token could be retrieve");
        }

        private void ValidateSettings(IKeyValueSettings settings)
        {
            if (null == settings)
                throw new ArgumentNullException(nameof(settings));

            var messages = new Messages();

            if (!settings.Values.ContainsKey(TokenKeys.ProviderIdKey))
                messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, "No Provider defined."));
            else if (!settings.Values[TokenKeys.ProviderIdKey].Equals(ProviderName))
                messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, $"Provider {settings.Values[TokenKeys.ProviderIdKey]} is not the expected one: {ProviderName}."));

            if (!settings.Values.ContainsKey("OboEndpointUrl"))
                messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, "No Obo url defined."));
            else if (!Uri.TryCreate(settings.Values["OboEndpointUrl"], UriKind.Absolute, out var uri))
                messages.Add(new Message(Arc4u.ServiceModel.MessageCategory.Technical, MessageType.Error, "Obo url is invalid."));

            messages.LogAndThrowIfNecessary(_logger);

            // I can do this because settings is registered in the DI as Shared => Singleton!
            _settings.Add(settings.GetHashCode(), settings);

        }

        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}

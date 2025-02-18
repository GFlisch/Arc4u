using System.Globalization;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arc4u.Configuration;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.IdentityModel.Claims;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Security.Principal;

[JsonSerializable(typeof(IEnumerable<ClaimDto>))]
internal partial class ClaimsProxyContext : JsonSerializerContext
{
}

[Export(typeof(IClaimsFiller))]
public class ClaimsProxy : IClaimsFiller
{
    public ClaimsProxy(IServiceProvider container, IAppSettings appSettings, IOptionsMonitor<ApplicationConfig> config, IHttpClientFactory httpClientFactory, ILogger<ClaimsProxy> logger)
    {
        _container = container;
        _httpClientFactory = httpClientFactory;
        _applicationName = config.CurrentValue?.ApplicationName ?? "Unknow";
        // read information to call the backend service from configuration.
        _url = appSettings.Values.ContainsKey("arc4u_ClaimsProxyUri") ? appSettings.Values["arc4u_ClaimsProxyUri"] : null;
        _logger = logger;
    }

    private string? _url;
    private readonly string _applicationName;
    protected readonly IServiceProvider _container;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClaimsProxy> _logger;

    public async Task<IEnumerable<ClaimDto>> GetAsync(IIdentity? identity, IEnumerable<IKeyValueSettings> settings, object? parameter)
    {
        var result = new List<ClaimDto>();

        if (null == identity)
        {
            _logger.Technical().LogNullIdentity();
            return result;
        }

        if (null == settings || !settings.Any())
        {
            _logger.Technical().LogNoTokenSettings();
            return result;
        }
        if (!settings.Any(s => s.Values.ContainsKey(TokenKeys.AuthenticationTypeKey) && s.Values[TokenKeys.AuthenticationTypeKey].Equals(identity.AuthenticationType)))
        {
            _logger.Technical().LogSkipFillingClaims(identity?.AuthenticationType ?? "No AuthenticationType");
            return result;
        }

        try
        {
            // Check before the url and application name is defined!
            if (string.IsNullOrWhiteSpace(_url))
            {
                // no override rule, use the standard endpoint defined.
                _url = settings.First().Values[TokenKeys.RootServiceUrlKey].TrimEnd('/') + "/api/claims";
            }
            else
            {
                _url = string.Format(CultureInfo.InvariantCulture, _url, _applicationName);
            }

            _logger.Technical().LogCallBackendWithUrl(_url);

            // Check if we need to do something before calling the backend like force the start of a vpn.
            Network.Handler.OnCalling?.Invoke(new Uri(_url));

            // call the backend service!
            var client = _httpClientFactory.CreateClient("ClaimsProxy");

            var response = await client.GetAsync(_url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                _logger.Technical().LogCallBackendWithUrlSucceed(_url);
                var responsestring = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                // Add the claims.
                var claims = JsonSerializer.Deserialize(responsestring, ClaimsProxyContext.Default.IEnumerableClaimDto);
                if (claims != null)
                {
                    result.AddRange(claims);
                    _logger.Technical().LogClaims(claims.Count());
                }
            }
            else
            {
                _logger.Technical().LogHttpStatusErrorCode(_url, (int)response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            var inner = exception.InnerException;
            while (null != inner)
            {
                _logger.Technical().LogException(inner);
                inner = inner.InnerException;
            }
            _logger.Technical().LogException(exception);
        }
        return result;
    }
}

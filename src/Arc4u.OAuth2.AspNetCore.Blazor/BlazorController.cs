using System.Net;
using System.Security.Claims;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.AspNetCore.Blazor;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Arc4u.AspNetCore.Results;
using Arc4u.OAuth2;

namespace Arc4u.Blazor;

/// <summary>
/// This controller is used to obtain an access token as a result of a back-end authentication and transmit it back to the Blazor application.
/// </summary>
/// <remarks>
/// This controller is known to the API Gateway (Yarp).
/// It needs a property called "RootServiceUrl" in the section Authentication:OpenId.Settings pointing to the yarp service URL.
/// </remarks>
[Authorize]
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
public class BlazorController : ControllerBase
{
    // The buffer size is set to 1024 bytes.
    private const int Buffer = 1024;

    // The provider to be used is "Oidc" (aka OpenId Connect).
    private const string ProviderId = "Oidc";

    // The root service URL key
    private const string RootServiceUrlKey = "Authentication:OpenId.Settings:RootServiceUrl";

    private readonly SimpleKeyValueSettings _settings;
    private readonly ILogger<BlazorController> _logger;
    private readonly string _rootServiceUrl;

    /// <summary>
    /// Default constructor
    /// </summary>
    public BlazorController(IOptionsSnapshot<SimpleKeyValueSettings> options, IConfiguration configuration, ILogger<BlazorController> logger)
    {
        _logger = logger;
        _settings = options.Get(Constants.CookiesAuthenticationType);
        _rootServiceUrl = configuration[RootServiceUrlKey] ?? throw new InvalidOperationException($"The root service URL is not defined in the configuration {RootServiceUrlKey}!");
    }

    /// <summary>
    /// This action is used to redirect the user to the Blazor application after having retrieved his/her access token.
    /// </summary>
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [HttpGet("redirectTo/{redirectTo}/{id?}")]
    public async Task<IActionResult> Get(int? id, string redirectTo, [FromServices] IApplicationContext applicationContext, [FromServices] IServiceProvider containerResolve, [FromServices] ILogger<BlazorController> logger)
    {
        if (applicationContext.Principal is null || applicationContext.Principal.Authorization.Operations.Count == 0)
        {
            _logger.LogUserNotIdentified();
            return Unauthorized();
        }

        string? accessToken = null;

        var index = id ?? 1;

        if (applicationContext.Principal.Identity is ClaimsIdentity claimsIdentity)
        {
            if (null != claimsIdentity.BootstrapContext)
            {
                accessToken = claimsIdentity.BootstrapContext.ToString();
            }
            else
            {
                if (containerResolve.TryGetService<ITokenProvider>(ProviderId, out var tokenProvider))
                {
                    if (tokenProvider is null)
                    {
                        _logger.Technical().LogTokenProviderIsNull(ProviderId);
                        return BadRequest();
                    }

                    var result = await tokenProvider!.GetTokenAsync(_settings, claimsIdentity).ConfigureAwait(false);
                    result.LogIfFailed();
                    accessToken = result.IsSuccess ? result.Value.Token : string.Empty;
                }
            }
        }

        // If the access token is null or empty, the user is not authenticated.
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.Technical().LogNoAccessToken();
            return BadRequest();
        }

        // The redirect URL is decoded and the redirect URI is built.
        var redirectUrl = WebUtility.UrlDecode(redirectTo);

        var url = $"https://{redirectUrl.TrimEnd('/')}/_content/Arc4u.Standard.OAuth2.Blazor/GetToken.html";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var redirectUri))
        {
            return new ObjectResult(new ProblemDetails()
                                            .WithTitle("Bad url.")
                                            .WithDetail($"Url {url} is not a valid one.")
                                            .WithStatusCode(StatusCodes.Status400BadRequest));
        }

        if (accessToken.Length > index * Buffer)
        {
            var thisController = _rootServiceUrl.TrimEnd('/') + $"/blazor/redirectto/{redirectTo}/{index + 1}&token={accessToken.Substring((index - 1) * Buffer, Buffer)}";
            return Redirect(UriHelper.Encode(new Uri($"{redirectUri}?url={thisController}")));
        }
        else
        {
            return Redirect($"{redirectUri}?token={accessToken.Substring((index - 1) * Buffer, accessToken.Length - (index - 1) * Buffer)}");
        }
    }
}

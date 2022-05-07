using Arc4u.Dependency;
using Arc4u.OAuth2.Aspect;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.Blazor
{
    /// <summary>
    ///  OboController class.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status400BadRequest)]
    public class BlazorController : ControllerBase
    {
        private const int buffer = 1024;

        public BlazorController(ILogger<BlazorController> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<BlazorController> _logger;

        /// <summary>
        /// This document will be part of the Sdk!
        /// </summary>
        /// <response code="2buffer">The Obo Bearer token</response>
        /// <returns>The Obo name.</returns>
        [ServiceAspect()]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [HttpGet("{id?}")]
        public async Task<IActionResult> Get(int? id, [FromServices] IApplicationContext applicationContext, [FromServices] IContainerResolve containerResolve)
        {
            if (applicationContext.Principal.Authorization.Operations.Count == 0) return Unauthorized();

            string accessToken = null;

            int index = id.HasValue ? id.Value : 1;

            if (applicationContext.Principal.Identity is ClaimsIdentity claimsIdentity)
            {
                if (null != claimsIdentity.BootstrapContext)
                {
                    accessToken = claimsIdentity.BootstrapContext.ToString();
                }
                else
                {
                    if (containerResolve.TryResolve<IKeyValueSettings>("OpenId", out var settings))
                    {
                        if (containerResolve.TryResolve<ITokenProvider>(settings.Values[TokenKeys.ProviderIdKey], out var tokenProvider))
                        {
                            accessToken = (await tokenProvider.GetTokenAsync(settings, claimsIdentity)).AccessToken;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var redirectUrl = Request.Query["redirectUrl"][0];

                var redirectUri = redirectUrl.TrimEnd('/') + "/_content/Arc4u.Standard.OAuth2.Blazor/GetToken.html";

                if (accessToken.Length > index * buffer)
                {
                    containerResolve.TryResolve<IKeyValueSettings>("OAuth2", out var settings);
                    var thisController = settings.Values[TokenKeys.RootServiceUrlKey].TrimEnd('/') + $"/blazor/{index + 1}&redirectUrl={redirectUrl}&token={accessToken.Substring((index - 1) * buffer, buffer)}";
                    return Redirect($"{redirectUri}?url={thisController}");
                }
                else
                {
                    return Redirect($"{redirectUri}?token={accessToken.Substring((index - 1) * buffer, accessToken.Length - (index - 1) * buffer)}");
                }
            }

            return BadRequest();
        }
    }
}

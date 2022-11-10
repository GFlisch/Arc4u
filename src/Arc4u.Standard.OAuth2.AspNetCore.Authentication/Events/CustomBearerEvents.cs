using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Events
{
    public class CustomBearerEvents : JwtBearerEvents
    {
        private readonly ILogger<CustomBearerEvents> _logger;
        public CustomBearerEvents(ILogger<CustomBearerEvents> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Log the exception, reason why the authentication is failing.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            _logger.Technical().Exception(context.Exception);

            context.Fail(context.Exception);
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Save the access token in the identity.BootstrapContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task TokenValidated(TokenValidatedContext context)
        {
            if (context.SecurityToken is JwtSecurityToken accessToken)
            {
                if (context.Principal.Identity is ClaimsIdentity identity)
                {
                    identity.BootstrapContext = accessToken.RawData;
                }
            }

            return Task.CompletedTask;
        }
    }
}

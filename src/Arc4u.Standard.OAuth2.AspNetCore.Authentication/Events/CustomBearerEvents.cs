using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Events
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
            context.Fail(context.Exception);
            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;
        }
    }
}

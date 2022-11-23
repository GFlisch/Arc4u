using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Aspect
{
    /// <summary>
    /// This attribute checks:
    /// - the security. if user is authenticated at least and if the necessary rihts are assigned to the user.
    /// - Keep the time to complete the call.
    /// - Allow to set or not the culture.
    /// Handle AppException or Exception and return a Bad RequestMessage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class ServiceAspectBase : ActionFilterAttribute, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// The page used to render when you are unauthorized.
        /// </summary>
        private readonly int[] _operations;
        private readonly String _scope = string.Empty;
        protected readonly ILogger Logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        private static Action<Type, TimeSpan> _log = null;

        public ServiceAspectBase(ILogger logger, IHttpContextAccessor httpContextAccessor, string scope, params int[] operations)
        {
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
            _scope = scope;
            _operations = operations;
        }

        public abstract void SetCultureInfo(ActionExecutingContext context);

        public static void SetExtraLogging(Action<Type, TimeSpan> log)
        {
            _log = log;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Thread.CurrentPrincipal = _httpContextAccessor.HttpContext.User;

            SetCultureInfo(context);

            await next().ConfigureAwait(true);
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var principal = _httpContextAccessor.HttpContext.User as Arc4u.Security.Principal.AppPrincipal;

            if (null != principal && principal.IsAuthorized(_scope, _operations))
            {
                return Task.CompletedTask;
            }

            context.Result = new UnauthorizedResult();

            return Task.CompletedTask;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

[Export(typeof(IApplicationAuthorizationPolicy)), Scoped]
public class ApplicationAuthorizationPolicy : IApplicationAuthorizationPolicy
{
    public ApplicationAuthorizationPolicy(IAuthorizationService authorizationService, IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
        _authorizationService = authorizationService;
    }

    private readonly IAuthorizationService _authorizationService;
    private readonly IApplicationContext _applicationContext;

    public async Task AuthorizeAsync(string policyName, [AllowNull] string? exceptionMessage = null)
    {
        if (!await IsAuthorizeAsync(policyName).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException(string.IsNullOrEmpty(exceptionMessage)
                ? $"User is not authorized to access the resource. Policy: {policyName}" : exceptionMessage);
        }
    }

    public async Task<bool> IsAuthorizeAsync(string policyName)
    {
        var authResult = await _authorizationService.AuthorizeAsync(_applicationContext.Principal, policyName).ConfigureAwait(false);

        return authResult.Succeeded;
    }
}

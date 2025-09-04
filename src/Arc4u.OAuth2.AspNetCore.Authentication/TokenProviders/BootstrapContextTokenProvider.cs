using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using FluentResults;

namespace Arc4u.OAuth2.TokenProviders
{
    [Export(BootstrapContextTokenProvider.ProviderName, typeof(ITokenProvider))]
    public class BootstrapContextTokenProvider(IApplicationContext applicationContext) : ITokenProvider
    {
        public const string ProviderName = "Bootstrap";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="platformParameters"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException" />
        /// <exception cref="TimeoutException" />
        /// <returns><see cref="TokenInfo"/></returns>
        public Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
        {
            ArgumentNullException.ThrowIfNull(settings);

            ArgumentNullException.ThrowIfNull(applicationContext.Principal);

            if (applicationContext.Principal.Identity is ClaimsIdentity identity && !string.IsNullOrWhiteSpace(identity?.BootstrapContext?.ToString()))
            {
                var token = identity.BootstrapContext.ToString();

                JwtSecurityToken jwt = new(token);

                if (jwt.ValidTo > DateTime.UtcNow)
                {
                    return Task.FromResult(new TokenInfo("Bearer", token!, jwt.ValidTo).ToResult());
                }

                return Task.FromResult<Result<TokenInfo>>(Result.Fail("The token provided is expired."));
            }

            return Task.FromResult<Result<TokenInfo>>(Result.Fail("No Access token stored in the Identity."));
        }

        /// <summary>
        /// There is no way to signout in this scenario.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}


namespace Arc4u.OAuth2.Token;

public interface ITokenRefreshProvider
{
    Task<TokenRefreshInfo?> RefreshTokenAsync(CancellationToken cancellationToken);
}

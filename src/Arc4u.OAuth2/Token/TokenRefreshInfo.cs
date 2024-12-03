namespace Arc4u.OAuth2.Token;

public record TokenRefreshInfo
{
    public TokenInfo AccessToken { get; set; } = default!;
    public TokenInfo RefreshToken { get; set; } = default!;

}

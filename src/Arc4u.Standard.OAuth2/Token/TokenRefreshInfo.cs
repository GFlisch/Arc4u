namespace Arc4u.OAuth2.Token;

public record TokenRefreshInfo
{
    public TokenInfo AccessToken { get; set; }
    public TokenInfo RefreshToken { get; set; }

}

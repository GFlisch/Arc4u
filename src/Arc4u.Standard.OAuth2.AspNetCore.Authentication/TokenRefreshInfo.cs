namespace Arc4u.Standard.OAuth2
{
    public record TokenRefreshInfo
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }
}

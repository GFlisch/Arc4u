namespace Arc4u.OAuth2.Net
{
    public interface IAuthorizationHeader
    {
        string GetHeader(IKeyValueSettings settings);
    }
}

namespace Arc4u.OAuth2.Token;

public interface ITokenCache
{
    /// <summary>
    /// Delete a token based on its unique key.
    /// </summary>
    /// <param name="key"></param>
    void DeleteItem(string key);

    void Put<T>(string key, T data);

    T? Get<T>(string key);
}

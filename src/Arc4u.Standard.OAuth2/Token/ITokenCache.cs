namespace Arc4u.OAuth2.Token
{
    public interface ITokenCache
    {
        /// <summary>
        /// Delete a token based on its unique key.
        /// </summary>
        /// <param name="Id"></param>
        void DeleteItem(string key);


        /// <summary>
        /// Set or overwrite the item with key <paramref name="key"/> with <paramref name="data"/>.
        /// The item expires after <paramref name="timeout"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="data"></param>
        void Put<T>(string key, TimeSpan timeout, T data);

        T Get<T>(string key);

    }
}

using Arc4u.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;

namespace Arc4u.OAuth2.Token.Adal
{
    /// <summary>
    /// Implement the TokenCache to use in an application.
    /// The class will delegate the cache to an external implementation!
    /// </summary>
    public class Cache : TokenCache
    {
        private readonly ITokenCache TokenCache;
        private readonly ILogger Logger;

        private String _identifier;


        public Cache(ILogger logger, IServiceProvider container, string identifier)
        {
            Logger = logger;

            TokenCache = container.GetService<ITokenCache>();
            if (TokenCache == null)
            {
                Logger.Technical().From<Cache>().Error("No implementation for an ITokenCache exists! Check your dependencies.").Log();
                Logger.Technical().From<Cache>().System($"Token cache is skipped for the user identifier: {identifier}.").Log();
            }

            _identifier = identifier;

            // Attach the events.
            BeforeAccess += BeforeAccessNotification;
            AfterAccess += AfterAccessNotification;


        }

        public override void Clear()
        {
            base.Clear();
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
        }

        public override IEnumerable<TokenCacheItem> ReadItems()
        {
            return base.ReadItems();
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Logger.Technical().From<Cache>().System($"Getting token info from the cache for identifier: {_identifier}.").Log();
            var tokenInfo = TokenCache?.Get<byte[]>(_identifier);

            if (default(byte[]) == tokenInfo)
            {
                Logger.Technical().From<Cache>().System($"Token info from the cache was received for identifier:{_identifier}.").Log();
                DeserializeAdalV3(null);
                return;
            }

            Logger.Technical().From<Cache>().System($"Deserializing the token information to generate a token from the cache for identifier: {_identifier}.").Log();
            DeserializeAdalV3(tokenInfo);
            Logger.Technical().From<Cache>().System($"Deserialized the token information for identifier: {_identifier}.").Log();
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (HasStateChanged)
            {
                Logger.Technical().From<Cache>().System($"Adding token information to the cache for the identifier: {_identifier}.").Log();
                TokenCache.Put(_identifier, SerializeAdalV3());
                Logger.Technical().From<Cache>().System($"Added token information to the cache for the identifier: {_identifier}.").Log();

                HasStateChanged = false;
            }
        }
    }
}

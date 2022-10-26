using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(OboTokenProvider.ProviderName, typeof(ITokenProvider))]

    public class AzureAdOboTokenProvider : OboTokenProvider
    {
        public AzureAdOboTokenProvider(CacheContext cacheContext, IServiceProvider container, ILogger<AzureAdOboTokenProvider> logger, IActivitySourceFactory activitySourceFactory) : base(cacheContext, container, logger, activitySourceFactory)
        { }

        protected override IConfidentialClientApplication CreateCca(IKeyValueSettings valueSettings)
        {
            return ConfidentialClientApplicationBuilder
                .Create(valueSettings.Values[TokenKeys.ClientIdKey])
                .WithAuthority(valueSettings.Values[TokenKeys.AuthorityKey])
                .WithClientSecret(valueSettings.Values[TokenKeys.ApplicationKey])
                .Build();
        }
    }
}

using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProviders;

[Export(AzureADOboTokenProvider.ProviderName, typeof(ITokenProvider))]
public class AzureADOboTokenProvider : ITokenProvider
{

    public AzureADOboTokenProvider(ILogger<AzureADOboTokenProvider> logger)
    {
        _logger = logger;
    }

    const string ProviderName = "Obo";


    private readonly ILogger<AzureADOboTokenProvider> _logger;

    public Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
    {



        throw new NotImplementedException();
    }

    public void SignOut(IKeyValueSettings settings)
    {
        throw new NotImplementedException();
    }
}

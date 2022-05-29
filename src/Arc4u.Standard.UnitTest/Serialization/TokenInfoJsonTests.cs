using Arc4u.OAuth2.Token;
using Arc4u.Standard.UnitTest.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Arc4u.Standard.UnitTest.Serialization
{
    public class TokenInfoJsonTests : BaseContainerFixture<TokenInfoJsonTests, TokenJsonSerializerFixture>
    {
        public TokenInfoJsonTests(TokenJsonSerializerFixture containerFixture) : base(containerFixture)
        {
        }


        [Fact]
        public void Test_TokenInfo_GetTokenInfo()
        {
            using (var container = Fixture.CreateScope())
            {
                LogStartBanner();

                var logger = container.Resolve<ILogger<TokenInfoJsonTests>>();

                var tokenCache = container.Resolve<ITokenCache>();

                var tokenInfo = new TokenInfo("Bearer",
                                              "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                                              "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
                                              DateTime.UtcNow);

                tokenCache.Put("key", tokenInfo);

                var cachedToken = tokenCache.Get<TokenInfo>("key");
                Assert.Equal("Bearer", cachedToken.AccessTokenType);
                Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", cachedToken.AccessToken);
                Assert.Equal("SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", cachedToken.IdToken);


                LogEndBanner();
            }
        }
    }
}

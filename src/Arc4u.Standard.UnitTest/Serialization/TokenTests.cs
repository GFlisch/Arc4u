using Arc4u.Caching.Memory;
using Arc4u.Dependency;
using Arc4u.OAuth2.Token;
using Arc4u.Serializer;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Arc4u.Standard.UnitTest.Blazor
{
    [Trait("Category", "CI")]
    public class TokenTests
    {
        public TokenTests()
        {
            fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());
        }
        
        private readonly Fixture fixture;


        [Fact]
        public void AccessTokenValidityShould()
        {
            // act
            JwtSecurityToken sut = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddHours(1));

            // assert
            (sut.ValidTo > DateTime.UtcNow.AddMinutes(-5)).Should().BeTrue();
        }

        [Fact]
        public void AccessTokenValidityShouldNot()
        {
            // act
            JwtSecurityToken sut = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

            // assert
            (sut.ValidTo > DateTime.UtcNow.AddMinutes(-5)).Should().BeFalse();
        }

        [Fact]
        public void AccessTokenBlazorSerializationShouldNot()
        {
            // Arrange
            JwtSecurityToken jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

            var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

            var sut = new Arc4u.Caching.SecureCache();
            sut.Initialize("store name");

            sut.Put("key", tokenInfo);

            // act
            var cachedToken = sut.Get<TokenInfo>("key");

            // assert
            cachedToken.Should().NotBeNull();
            cachedToken.Token.Should().Be(jwt.EncodedPayload);

        }

        [Fact]
        public void AccessTokenSerializationJsonShouldNot()
        {
            // Arrange
            JwtSecurityToken jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

            var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

            var storeName = "store name";
            Dictionary<String, String> keySettings = new();
            keySettings.Add(MemoryCache.CompactionPercentageKey, "20");
            keySettings.Add(MemoryCache.SizeLimitKey, "1");
            
            var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
            mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

            IKeyValueSettings settings = mockKeyValueSettings.Object;

            var mockContainer = fixture.Freeze<Mock<IContainerResolve>>();
            mockContainer.Setup((m) => m.TryResolve<IKeyValueSettings>(storeName, out settings)).Returns(true);

            IObjectSerialization noSerializer = null;
            IObjectSerialization serializer = new JsonSerialization();
            mockContainer.Setup(m => m.TryResolve<IObjectSerialization>(storeName, out noSerializer)).Returns(false);
            mockContainer.Setup(m => m.Resolve<IObjectSerialization>()).Returns(serializer);

            var sut = fixture.Create<MemoryCache>();
            sut.Initialize("store name");

            sut.Put("key", tokenInfo);

            // act
            var cachedToken = sut.Get<TokenInfo>("key");

            // assert
            cachedToken.Should().NotBeNull();
            cachedToken.Token.Should().Be(jwt.EncodedPayload);
            cachedToken.TokenType.Should().Be(tokenInfo.TokenType);
            cachedToken.ExpiresOnUtc.Should().Be(tokenInfo.ExpiresOnUtc);          
        }

        [Fact]
        public void AccessTokenSerializationProtobufShouldNot()
        {
            // Arrange
            JwtSecurityToken jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

            var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

            var storeName = "store name";
            Dictionary<String, String> keySettings = new();
            keySettings.Add(MemoryCache.CompactionPercentageKey, "20");
            keySettings.Add(MemoryCache.SizeLimitKey, "1");

            var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
            mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

            IKeyValueSettings settings = mockKeyValueSettings.Object;

            var mockContainer = fixture.Freeze<Mock<IContainerResolve>>();
            mockContainer.Setup((m) => m.TryResolve<IKeyValueSettings>(storeName, out settings)).Returns(true);

            IObjectSerialization noSerializer = null;
            IObjectSerialization serializer = new ProtoBufSerialization();
            mockContainer.Setup(m => m.TryResolve<IObjectSerialization>(storeName, out noSerializer)).Returns(false);
            mockContainer.Setup(m => m.Resolve<IObjectSerialization>()).Returns(serializer);

            var sut = fixture.Create<MemoryCache>();
            sut.Initialize("store name");

            sut.Put("key", tokenInfo);

            // act
            var cachedToken = sut.Get<TokenInfo>("key");

            // assert
            cachedToken.Should().NotBeNull();
            cachedToken.Token.Should().Be(jwt.EncodedPayload);
            cachedToken.TokenType.Should().Be(tokenInfo.TokenType);
            cachedToken.ExpiresOnUtc.Should().Be(tokenInfo.ExpiresOnUtc);
        }

        [Fact]
        public void AccessTokenSerializationProtobufZipShouldNot()
        {
            // Arrange
            JwtSecurityToken jwt = new JwtSecurityToken("issuer", "audience", new List<Claim> { new Claim("key", "value") }, notBefore: DateTime.UtcNow.AddHours(-1), expires: DateTime.UtcNow.AddMinutes(-10));

            var tokenInfo = new TokenInfo("Bearer", jwt.EncodedPayload, DateTime.UtcNow);

            var storeName = "store name";
            Dictionary<String, String> keySettings = new();
            keySettings.Add(MemoryCache.CompactionPercentageKey, "20");
            keySettings.Add(MemoryCache.SizeLimitKey, "1");

            var mockKeyValueSettings = fixture.Freeze<Mock<IKeyValueSettings>>();
            mockKeyValueSettings.SetupGet(p => p.Values).Returns(keySettings);

            IKeyValueSettings settings = mockKeyValueSettings.Object;

            var mockContainer = fixture.Freeze<Mock<IContainerResolve>>();
            mockContainer.Setup((m) => m.TryResolve<IKeyValueSettings>(storeName, out settings)).Returns(true);

            IObjectSerialization noSerializer = null;
            IObjectSerialization serializer = new ProtoBufZipSerialization();
            mockContainer.Setup(m => m.TryResolve<IObjectSerialization>(storeName, out noSerializer)).Returns(false);
            mockContainer.Setup(m => m.Resolve<IObjectSerialization>()).Returns(serializer);

            var sut = fixture.Create<MemoryCache>();
            sut.Initialize("store name");

            sut.Put("key", tokenInfo);

            // act
            var cachedToken = sut.Get<TokenInfo>("key");

            // assert
            cachedToken.Should().NotBeNull();
            cachedToken.Token.Should().Be(jwt.EncodedPayload);
            cachedToken.TokenType.Should().Be(tokenInfo.TokenType);
            cachedToken.ExpiresOnUtc.Should().Be(tokenInfo.ExpiresOnUtc);
        }
    }
}

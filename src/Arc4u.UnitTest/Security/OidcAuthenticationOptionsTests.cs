using AutoFixture.AutoMoq;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Xunit;
using FluentAssertions;
using Arc4u.Configuration;
using AutoFixture.Kernel;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Arc4u.Security.Cryptography;
using Moq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Arc4u.OAuth2.Events;
using Arc4u.Dependency;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class OidcAuthenticationOptionsTests
{
    public OidcAuthenticationOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _fixture.Customizations.Add(new X509Certificate2SpecimenBuilder());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Empty_Oidc_Authentication_Should()
    {
        var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                       new Dictionary<string, string?>
                       {
                       }).Build();

        IConfiguration configuration = new ConfigurationRoot([.. config.Providers]);

        IServiceCollection services = new ServiceCollection();

        var exception = Record.Exception(() => AuthenticationExtensions.AddOidcAuthentication(services, configuration));

        exception.Should().BeOfType<ConfigurationException>();
    }

    [Fact]
    public void Default_Oidc_Authentication_Value_Should()
    {
        var oidcSettings = _fixture.Create<OidcAuthenticationOptions>();
        var defaultSettings = new OidcAuthenticationOptions();
        var tokenCacheSettings = _fixture.Create<TokenCacheOptions>();
        var defaultAuthority = _fixture.Create<AuthorityOptions>();
        var auth2Options = _fixture.Create<OAuth2SettingsOption>();
        var oidcOptions = _fixture.Create<OpenIdSettingsOption>();

        var configDic = new Dictionary<string, string?>
        {
            { "Application.configuration:ApplicationName", "TestName" },
            { "Authentication:DefaultAuthority:RetryInterval", defaultAuthority.RetryInterval.ToString() },
            { "Authentication:DefaultAuthority:MetaDataAddress", defaultAuthority.MetaDataAddress!.ToString() },
            { "Authentication:DefaultAuthority:Url", defaultAuthority.Url.ToString() },
            { "Authentication:DefaultAuthority:TokenEndpoint", defaultAuthority.TokenEndpoint!.ToString() },
            { "Authentication:CookieName", oidcSettings.CookieName },
            { "Authentication:ApplicationName", oidcSettings.ApplicationName },
            { "Authentication:TokenCache:CacheName", tokenCacheSettings.CacheName },
            { "Authentication:ValidateAudience", false.ToString() },
            { "Authentication:DataProtection:CacheStore:CacheKey", "TokenCacheKey" },
            { "Authentication:DataProtection:CacheStore:CacheName", "TokenCacheName" },

            // OpenId
            { "Authentication:OpenId.Settings:ClientId", oidcOptions.ClientId },
            { "Authentication:OpenId.Settings:ClientSecret", oidcOptions.ClientSecret },
        };

        // OAuth2
        foreach (var audience in auth2Options.Audiences)
        {
            configDic.Add($"Authentication:OAuth2.Settings:Audiences:{auth2Options.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in auth2Options.Scopes)
        {
            configDic.Add($"Authentication:OAuth2.Settings:Scopes:{auth2Options.Scopes.IndexOf(scope)}", scope);
        }

        // OpenId
        foreach (var audience in oidcOptions.Audiences)
        {
            configDic.Add($"Authentication:OpenId.Settings:Audiences:{oidcOptions.Audiences.IndexOf(audience)}", audience);
        }
        foreach (var scope in oidcOptions.Scopes)
        {
            configDic.Add($"Authentication:OpenId.Settings:Scopes:{oidcOptions.Scopes.IndexOf(scope)}", scope);
        }

        var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDic).Build();

        IConfiguration configuration = new ConfigurationRoot([.. config.Providers]);

        IServiceCollection services = new ServiceCollection();

        var mockCertificateLoader = new Mock<IX509CertificateLoader>();
        mockCertificateLoader.Setup(loader => loader.FindCertificate(It.IsAny<IConfiguration>(), It.IsAny<string>()))
                             .Returns(_fixture.Create<X509Certificate2>());

        AuthenticationExtensions.AddOidcAuthentication(services, configuration, certificateLoader: mockCertificateLoader.Object);

        var app = services.BuildServiceProvider();

        var sut = app.GetRequiredService<IOptionsMonitor<OidcAuthenticationOptions>>().Get("Default");

        sut.Should().NotBeNull();
    }

    [Fact]
    public void Test_GetType_From_ServiceCollection()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<StandardBearerEvents>();

        var type = services.GetImplementationType<JwtBearerEvents>();
        type.Should().Be(typeof(StandardBearerEvents));

    }

    public class X509Certificate2SpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(X509Certificate2))
            {
                // Create a self-signed certificate for testing purposes
                var ecdsa = ECDsa.Create();
                var req = new CertificateRequest("cn=TestCertificate", ecdsa, HashAlgorithmName.SHA256);
                var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
                return cert;
            }

            return new NoSpecimen();
        }
    }
}

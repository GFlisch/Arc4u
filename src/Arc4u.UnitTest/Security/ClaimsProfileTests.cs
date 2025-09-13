using System.Security.Claims;
using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.Security.Principal;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using ClaimTypes = Arc4u.IdentityModel.Claims.ClaimTypes;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class ClaimsProfileTests
{
    private readonly Fixture _fixture;

    public ClaimsProfileTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void ProfileUpnFillerShould()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:DomainsMapping:Arc4u.net"] = "arc4u.net",
                    ["Authentication:DomainsMapping:Arc4u"] = "arc4u.net",
                    ["Authentication:DomainsMapping:Arc4u-net"] = "arc4u.net"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>()
            .Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Culture, "fr-BE"),
            new Claim(ClaimTypes.Name, "Flisch"),
            new Claim(ClaimTypes.GivenName, "Gilles"),
            new Claim(ClaimTypes.Email, "info@arc4u.net"),
            new Claim(ClaimTypes.Upn, "info@arc4u.net"),
            new Claim(ClaimTypes.Company, "Arc4u"),
            new Claim(ClaimTypes.Sid, Guid.NewGuid().ToS19())
        ], "TestType");

        var sut = profileFiller.GetProfile(identity);

        sut.Should().NotBeNull();
        mockSettings.Verify(m => m.Get("DomainMapping"), Times.Once());
        sut.Domain.Should().Be(settings.Values["Arc4u.net"]);
    }

    [Fact]
    public void ProfileFillerShould()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:DomainsMapping:Arc4u.net"] = "arc4u.net",
                    ["Authentication:DomainsMapping:arc4u"] = "arc4u.net",
                    ["Authentication:DomainsMapping:Arc4u-net"] = "arc4u.net"
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>()
            .Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Culture, "fr-BE"),
            new Claim(ClaimTypes.Name, "Flisch"),
            new Claim(ClaimTypes.GivenName, "Gilles"),
            new Claim(ClaimTypes.Email, "info@arc4u.net"),
            new Claim(ClaimTypes.Upn, "Arc4u\\info"),
            new Claim(ClaimTypes.Company, "Arc4u"),
            new Claim(ClaimTypes.Sid, Guid.NewGuid().ToS19())
        ], "TestType");

        var sut = profileFiller.GetProfile(identity);

        sut.Should().NotBeNull();
        mockSettings.Verify(m => m.Get("DomainMapping"), Times.Once());
        sut.Domain.Should().Be(settings.Values["Arc4u.net"]);
    }

    [Fact]
    public void ProfileWithNoDomainMappingFillerShould()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                []).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>()
            .Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Culture, "fr-BE"),
            new Claim(ClaimTypes.Name, "Flisch"),
            new Claim(ClaimTypes.GivenName, "Gilles"),
            new Claim(ClaimTypes.Email, "info@arc4u.net"),
            new Claim(ClaimTypes.Upn, "info@arc4u.net"),
            new Claim(ClaimTypes.Company, "Arc4u"),
            new Claim(ClaimTypes.Sid, Guid.NewGuid().ToS19())
        ], "TestType");

        var sut = profileFiller.GetProfile(identity);

        sut.Should().NotBeNull();
        mockSettings.Verify(m => m.Get("DomainMapping"), Times.Once());
        sut.Domain.Should().Be("arc4u");
    }
}

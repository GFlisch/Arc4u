using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Options;
using Moq;
using Arc4u.Configuration;
using System.Collections.Generic;
using Arc4u.Security.Principal;
using System.Security.Claims;
using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Arc4u.OAuth2.Extensions;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]

public class ClaimsProfileTests
{
    public ClaimsProfileTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void ProfileUpnFillerShould()
    {
        var config = new ConfigurationBuilder()
             .AddInMemoryCollection(
        new Dictionary<string, string?>
        {
            ["Authentication:DomainsMapping:Arc4u.net"] = "arc4u.net",
            ["Authentication:DomainsMapping:Arc4u"] = "arc4u.net",
            ["Authentication:DomainsMapping:Arc4u-net"] = "arc4u.net",
        }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>().Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(new List<Claim>()
        {
            new Claim(IdentityModel.Claims.ClaimTypes.Culture, "fr-BE"),
            new Claim(IdentityModel.Claims.ClaimTypes.Name, "Flisch"),
            new Claim(IdentityModel.Claims.ClaimTypes.GivenName, "Gilles"),
            new Claim(IdentityModel.Claims.ClaimTypes.Email, "info@arc4u.net"),
            new Claim(IdentityModel.Claims.ClaimTypes.Upn, "info@arc4u.net"),
            new Claim(IdentityModel.Claims.ClaimTypes.Company, "Arc4u"),
            new Claim(IdentityModel.Claims.ClaimTypes.Sid, Guid.NewGuid().ToS19())
        }, "TestType");

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
            ["Authentication:DomainsMapping:Arc4u-net"] = "arc4u.net",
        }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>().Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(new List<Claim>()
        {
            new Claim(IdentityModel.Claims.ClaimTypes.Culture, "fr-BE"),
            new Claim(IdentityModel.Claims.ClaimTypes.Name, "Flisch"),
            new Claim(IdentityModel.Claims.ClaimTypes.GivenName, "Gilles"),
            new Claim(IdentityModel.Claims.ClaimTypes.Email, "info@arc4u.net"),
            new Claim(IdentityModel.Claims.ClaimTypes.Upn, "Arc4u\\info"),
            new Claim(IdentityModel.Claims.ClaimTypes.Company, "Arc4u"),
            new Claim(IdentityModel.Claims.ClaimTypes.Sid, Guid.NewGuid().ToS19())
        }, "TestType");

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
        new Dictionary<string, string?>
        {

        }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddDomainMapping(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var settings = serviceProvider.GetRequiredService<IOptionsMonitor<SimpleKeyValueSettings>>().Get("DomainMapping");

        var mockSettings = _fixture.Freeze<Mock<IOptionsMonitor<SimpleKeyValueSettings>>>();
        mockSettings.Setup(m => m.Get("DomainMapping")).Returns(settings).Verifiable();

        var profileFiller = _fixture.Create<ClaimsProfileFiller>();

        var identity = new ClaimsIdentity(new List<Claim>()
        {
            new Claim(IdentityModel.Claims.ClaimTypes.Culture, "fr-BE"),
            new Claim(IdentityModel.Claims.ClaimTypes.Name, "Flisch"),
            new Claim(IdentityModel.Claims.ClaimTypes.GivenName, "Gilles"),
            new Claim(IdentityModel.Claims.ClaimTypes.Email, "info@arc4u.net"),
            new Claim(IdentityModel.Claims.ClaimTypes.Upn, "info@arc4u.net"),
            new Claim(IdentityModel.Claims.ClaimTypes.Company, "Arc4u"),
            new Claim(IdentityModel.Claims.ClaimTypes.Sid, Guid.NewGuid().ToS19())
        }, "TestType");

        var sut = profileFiller.GetProfile(identity);

        sut.Should().NotBeNull();
        mockSettings.Verify(m => m.Get("DomainMapping"), Times.Once());
        sut.Domain.Should().Be("arc4u");
    }
}

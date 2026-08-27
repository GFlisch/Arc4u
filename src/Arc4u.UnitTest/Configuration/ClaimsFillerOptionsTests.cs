using Arc4u.OAuth2;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest;

[Trait("Category", "CI")]
public class ClaimsFillerOptionsTests
{
    private readonly Fixture _fixture;

    public ClaimsFillerOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void Default_Settings_Should()
    {
        var _default = new ClaimsFillerOptions();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClaimsMiddleWare:ClaimsFiller:LoadClaimsFromClaimsFillerProvider"] =
                        _default.LoadClaimsFromClaimsFillerProvider.ToString()
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsFillerOptions>>();

        sut.Should().NotBeNull();
        sut!.Value.Should().NotBeNull();
        sut.Value.LoadClaimsFromClaimsFillerProvider.Should().Be(_default.LoadClaimsFromClaimsFillerProvider);
        sut.Value.SettingsKeys.Should().Equal(Constants.OpenIdOptionsName);
        sut.Value.ExpireClaim.Should().Be(_default.ExpireClaim);
        sut.Value.ClaimsToExclude.Should().Equal(AddClaimsFillerExtension.DefaultClaimsToExclude);
    }

    [Fact]
    public void Random_Settings_Should()
    {
        var options = _fixture.Create<ClaimsFillerOptions>();

        var settingsKeysDictionary = options.SettingsKeys
            .Select((value, index) => new { value, index })
            .ToDictionary(
                x => $"Authentication:ClaimsMiddleWare:ClaimsFiller:SettingsKeys:{x.index}", // Key: e.g. "SettingKey_0"
                x => (string?)x.value // Value: original value
            );
        var claimsToExcludeKeysDictionary = options.ClaimsToExclude
            .Select((value, index) => new { value, index })
            .ToDictionary(
                x =>
                    $"Authentication:ClaimsMiddleWare:ClaimsFiller:ClaimsToExclude:{x.index}", // Key: e.g. "SettingKey_0"
                x => (string?)x.value // Value: original value
            );
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                    {
                        ["Authentication:ClaimsMiddleWare:ClaimsFiller:LoadClaimsFromClaimsFillerProvider"] =
                            options.LoadClaimsFromClaimsFillerProvider.ToString(),
                        ["Authentication:ClaimsMiddleWare:ClaimsFiller:ExpireClaim"] = options.ExpireClaim
                    }
                    .Union(claimsToExcludeKeysDictionary)
                    .Union(settingsKeysDictionary)).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsFillerOptions>>();

        sut.Should().NotBeNull();
        sut!.Value.Should().NotBeNull();
        sut.Value.LoadClaimsFromClaimsFillerProvider.Should().Be(options.LoadClaimsFromClaimsFillerProvider);
        sut.Value.SettingsKeys.Should().Equal(options.SettingsKeys);
        sut.Value.ExpireClaim.Should().Be(options.ExpireClaim);
        sut.Value.ClaimsToExclude.Should().Equal(options.ClaimsToExclude);
    }

    [Fact]
    public void Set_No_Claims_To_Exclude_Should()
    {
        var _default = new ClaimsFillerOptions();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:ClaimsMiddleWare:ClaimsFiller:LoadClaimsFromClaimsFillerProvider"] =
                        _default.LoadClaimsFromClaimsFillerProvider.ToString(),
                    ["Authentication:ClaimsMiddleWare:ClaimsFiller:ClaimsToExclude"] = ""
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsFillerOptions>>();

        sut.Should().NotBeNull();
        sut!.Value.Should().NotBeNull();
        sut.Value.LoadClaimsFromClaimsFillerProvider.Should().Be(_default.LoadClaimsFromClaimsFillerProvider);
        sut.Value.SettingsKeys.Should().Equal(Constants.OpenIdOptionsName);
        sut.Value.ExpireClaim.Should().Be(_default.ExpireClaim);
        sut.Value.ClaimsToExclude.Should().BeEmpty();
    }

    [Fact]
    public void Default_Config_When_No_Section_Should()
    {
        var _default = new ClaimsFillerOptions();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        IServiceCollection services = new ServiceCollection();

        services.AddClaimsFiller(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetService<IOptions<ClaimsFillerOptions>>();

        sut.Should().NotBeNull();
        sut!.Value.Should().NotBeNull();
        sut.Value.LoadClaimsFromClaimsFillerProvider.Should().Be(_default.LoadClaimsFromClaimsFillerProvider);
        sut.Value.SettingsKeys.Should().Equal(Constants.OpenIdOptionsName);
        sut.Value.ExpireClaim.Should().Be(_default.ExpireClaim);
        sut.Value.ClaimsToExclude.Should().Equal(AddClaimsFillerExtension.DefaultClaimsToExclude);
    }
}

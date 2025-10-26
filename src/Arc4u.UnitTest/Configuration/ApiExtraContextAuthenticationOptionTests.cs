using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Arc4u.UnitTest.Configuration;

[Trait("Category", "CI")]
public class ApiExtraContextAuthenticationOptionTests
{
    private readonly Fixture _fixture;

    public ApiExtraContextAuthenticationOptionTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    [Fact]
    public void ApiExtraContextAuthenticationOption_Action_Should()
    {
        var services = new ServiceCollection();
        services.AddAuthenticationApiContext(option =>
        {
            option.AuthorizationParameters = new SimpleKeyValueSettings(new Dictionary<string, string>() { ["audience"] = "http://arc4u/auth" });
            option.TokenParameters = new SimpleKeyValueSettings(new Dictionary<string, string>() { ["response"] = "http://arc4u/token" });
        });

        var provider = services.BuildServiceProvider();

        var option = provider.GetService<IOptions<ApiExtraContextAuthenticationOption>>();

        Assert.NotNull(option);
        var parameters = option.Value;

        Assert.NotNull(parameters);
        Assert.NotNull(parameters.AuthorizationParameters);
        Assert.NotEmpty(parameters.AuthorizationParameters.Values);
        Assert.Equal("http://arc4u/auth", parameters.AuthorizationParameters.Values["audience"]);
        Assert.NotNull(parameters.TokenParameters);
        Assert.NotEmpty(parameters.TokenParameters.Values);
        Assert.Equal("http://arc4u/token", parameters.TokenParameters.Values["response"]);
    }

    [Fact]
    public void ApiExtraContextAuthenticationOption_Empty_Action_Should()
    {
        var services = new ServiceCollection();
        services.AddAuthenticationApiContext(_ => { });

        var provider = services.BuildServiceProvider();

        var option = provider.GetService<IOptions<ApiExtraContextAuthenticationOption>>();

        Assert.NotNull(option);
        var parameters = option.Value;

        Assert.NotNull(parameters);
        Assert.NotNull(parameters.AuthorizationParameters);
        Assert.Empty(parameters.AuthorizationParameters.Values);
        Assert.NotNull(parameters.TokenParameters);
        Assert.Empty(parameters.TokenParameters.Values);
    }

    [Fact]
    public void ApiExtraContextAuthenticationOption_From_Config_Should()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:OpenId.Settings:AuthorizationEndpoint:audience"] = "http://arc4u/aud",
                    ["Authentication:OpenId.Settings:AuthorizationEndpoint:resource"] = "http://arc4u/res",
                    ["Authentication:OpenId.Settings:TokenEndpoint:audience"] = "http://guidance/aud",
                    ["Authentication:OpenId.Settings:TokenEndpoint:resource"] = "http://guidance/res",

                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        services.AddAuthenticationApiContext(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var option = serviceProvider.GetService<IOptions<ApiExtraContextAuthenticationOption>>();

        Assert.NotNull(option);
        var parameters = option.Value;

        Assert.NotNull(parameters);
        Assert.NotNull(parameters.AuthorizationParameters);
        Assert.NotEmpty(parameters.AuthorizationParameters.Values);
        Assert.Equal(2, parameters.AuthorizationParameters.Values.Count);
        Assert.Equal("http://arc4u/aud", parameters.AuthorizationParameters.Values["audience"]);
        Assert.Equal("http://arc4u/res", parameters.AuthorizationParameters.Values["resource"]);
        Assert.NotNull(parameters.TokenParameters);
        Assert.NotEmpty(parameters.TokenParameters.Values);
        Assert.Equal(2, parameters.TokenParameters.Values.Count);
        Assert.Equal("http://guidance/aud", parameters.TokenParameters.Values["audience"]);
        Assert.Equal("http://guidance/res", parameters.TokenParameters.Values["resource"]);

    }

    [Fact]
    public void ApiExtraContextAuthenticationOption_From_No_Config_Should()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        services.AddAuthenticationApiContext(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var option = serviceProvider.GetService<IOptions<ApiExtraContextAuthenticationOption>>();

        Assert.NotNull(option);
        var parameters = option.Value;

        Assert.NotNull(parameters);
        Assert.NotNull(parameters.AuthorizationParameters);
        Assert.Empty(parameters.AuthorizationParameters.Values);
        Assert.NotNull(parameters.TokenParameters);
        Assert.Empty(parameters.TokenParameters.Values);
    }

    [Fact]
    public void ApiExtraContextAuthenticationOption_One_Config_Should()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                }).Build();

        IConfiguration configuration = new ConfigurationRoot(new List<IConfigurationProvider>(config.Providers));

        services.AddAuthenticationApiContext(configuration);

        var hasConfiguration = services.Any(x =>
            x.ServiceType == typeof(IConfigureOptions<ApiExtraContextAuthenticationOption>) ||
            x.ServiceType == typeof(IPostConfigureOptions<ApiExtraContextAuthenticationOption>) ||
            x.ServiceType == typeof(IValidateOptions<ApiExtraContextAuthenticationOption>));

        Assert.True(hasConfiguration);
    }
}

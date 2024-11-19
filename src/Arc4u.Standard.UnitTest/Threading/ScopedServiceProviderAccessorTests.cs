using Arc4u.OAuth2.AspNetCore;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Arc4u.UnitTest.Threading;

public class ScopedServiceProviderAccessorTests
{
    public ScopedServiceProviderAccessorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Scoped_Service_Accessor_When_Null_Should()
    {
        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);

        var exception = Record.Exception(() => sut.ServiceProvider);

        exception.Should().BeOfType<NullReferenceException>();
    }

    [Fact]
    public void Scoped_Service_Accessor_When_HttpContextAccessor_Exists_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        var serviceScopedProvider = serviceProvider.CreateScope().ServiceProvider;
        var mockHttpContext = _fixture.Freeze<Mock<HttpContext>>();
        mockHttpContext.SetupGet<IServiceProvider>(g => g.RequestServices).Returns(() => serviceScopedProvider);

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => mockHttpContext.Object);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);

        sut.ServiceProvider.Should().NotBeNull();
        sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider);
    }

    [Fact]
    public void Scoped_Service_Accessor_When_HttpContextAccessor_Doesnt_Exist_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        var serviceScopedProvider = serviceProvider.CreateScope().ServiceProvider;
        var mockHttpContext = _fixture.Freeze<Mock<HttpContext>>();
        mockHttpContext.SetupGet<IServiceProvider>(g => g.RequestServices).Returns(() => null);

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => mockHttpContext.Object);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object)
        {
            ServiceProvider = serviceScopedProvider
        };

        sut.ServiceProvider.Should().NotBeNull();
        sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider);
    }

    [Fact]
    public void Scoped_Service_Accessor_When_HttpContextAccessor_And_ServiceProvider_Exists_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var serviceScopedProvider1 = serviceProvider.CreateScope().ServiceProvider;
        var mockHttpContext = _fixture.Freeze<Mock<HttpContext>>();
        mockHttpContext.SetupGet<IServiceProvider>(g => g.RequestServices).Returns(() => serviceScopedProvider1);

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => mockHttpContext.Object);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);
        var serviceScopedProvider2 = serviceProvider.CreateScope().ServiceProvider;
        sut.ServiceProvider = serviceScopedProvider2;

        sut.ServiceProvider.Should().NotBeNull();
        sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider2);
    }

    [Fact]
    public async void Scoped_Service_Accessor_When_HttpContextAccessor_And_ServiceProvider_Exists_In_A_Task_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var serviceScopedProvider1 = serviceProvider.CreateScope().ServiceProvider;
        var mockHttpContext = _fixture.Freeze<Mock<HttpContext>>();
        mockHttpContext.SetupGet<IServiceProvider>(g => g.RequestServices).Returns(() => serviceScopedProvider1);

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => mockHttpContext.Object);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);

        sut.ServiceProvider.Should().NotBeNull();
        sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider1);

        await Task.Run(() =>
        {
            sut.ServiceProvider.Should().NotBeNull();
            sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider1);

            using var serviceScope = serviceProvider.CreateScope();
            var serviceProvider1 = serviceScope.ServiceProvider;
            sut.ServiceProvider = serviceProvider1;

            sut.ServiceProvider.Should().NotBeNull();
            sut.ServiceProvider.Should().BeSameAs(serviceProvider1);
        }).ConfigureAwait(false);

        sut.ServiceProvider.Should().NotBeNull();
        sut.ServiceProvider.Should().BeSameAs(serviceScopedProvider1);

    }

    [Fact]
    public async void Scoped_Service_Accessor_When_HttpContextAccessor_Is_Null_And_ServiceProvider_Exists_In_A_Task_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => null);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);

        var exception = Record.Exception(() => sut.ServiceProvider);
        exception.Should().BeOfType<NullReferenceException>();

        await Task.Run(() =>
        {
            exception = Record.Exception(() => sut.ServiceProvider);
            exception.Should().BeOfType<NullReferenceException>();

            using var serviceScope = serviceProvider.CreateScope();
            var serviceProvider1 = serviceScope.ServiceProvider;
            sut.ServiceProvider = serviceProvider1;

            sut.ServiceProvider.Should().NotBeNull();
            sut.ServiceProvider.Should().BeSameAs(serviceProvider1);
        }).ConfigureAwait(false);

        exception = Record.Exception(() => sut.ServiceProvider);
        exception.Should().BeOfType<NullReferenceException>();

    }

    [Fact]
    public void Scoped_Service_Accessor_When_ServiceProvider_Is_Not_Scoped_Should()
    {
        IServiceCollection services = new ServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var mockHttpContext = _fixture.Freeze<Mock<HttpContext>>();
        mockHttpContext.SetupGet<IServiceProvider>(g => g.RequestServices).Returns(() => null);

        var mockHttpContextAccessor = _fixture.Freeze<Mock<IHttpContextAccessor>>();
        mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(() => mockHttpContext.Object);

        var sut = new ScopedServiceProviderAccessor(mockHttpContextAccessor.Object);
        var exception = Record.Exception(() => sut.ServiceProvider = serviceProvider);

        exception.Should().BeOfType<ArgumentException>();
    }
}

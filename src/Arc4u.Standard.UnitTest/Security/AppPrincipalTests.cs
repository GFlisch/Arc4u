using Arc4u.Extensions;
using Arc4u.Security.Principal;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class AppPrincipalTests
{
    public AppPrincipalTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    private enum Access : int
    {
        AccessApplication = 1,
        CanSeeSwaggerFacadeApi = 2
    }

    [Fact]
    public void Test_IsAuthorise_By_Enum_Should()
    {
        var authorization = GetAuthorization();

        var appAuthorization = new AppAuthorization(authorization);

        appAuthorization.IsAuthorized(Access.AccessApplication).Should().BeTrue();
    }

    [Fact]
    public void Test_IsAuthorise_By_Enum_Should_Not()
    {
        var authorization = GetAuthorization();

        var appAuthorization = new AppAuthorization(authorization);

        appAuthorization.IsAuthorized(Access.AccessApplication, Access.CanSeeSwaggerFacadeApi).Should().BeFalse();
    }

    [Fact]
    public void Test_IsAuthorise_By_Enum_Should_For_Specific()
    {
        var authorization = GetAuthorization();

        var appAuthorization = new AppAuthorization(authorization);

        appAuthorization.IsAuthorized("Specific", Access.AccessApplication, Access.CanSeeSwaggerFacadeApi).Should().BeTrue();
    }

    private static Authorization GetAuthorization()
    {
        var defaultScopedOperations = new ScopedOperations { Operations = [(int)Access.AccessApplication], Scope = "" };
        var specificScope = new ScopedOperations { Operations = [(int)Access.AccessApplication, (int)Access.CanSeeSwaggerFacadeApi], Scope = "Specific" };
        var authorization = new Authorization
        {
            Operations = [defaultScopedOperations, specificScope],
            AllOperations = AllOperations,
            Roles = [new ScopedRoles { Roles = ["User"], Scope = "" }],
            Scopes = ["", "Specific"]
        };

        return authorization;
    }
    private static List<Operation> AllOperations => System.Enum.GetValues<Access>().Select(o => new Operation { Name = o.GetValue(), ID = (int)o }).ToList();
}

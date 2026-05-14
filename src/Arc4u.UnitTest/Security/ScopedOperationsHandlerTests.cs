using System.Security.Claims;
using Arc4u.Authorization;
using Arc4u.Security.Principal;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class ScopedOperationsHandlerTests
{
    private static AppPrincipal CreatePrincipal(params ScopedOperations[] scopedOperations)
    {
        var allOperationIds = scopedOperations
            .SelectMany(s => s.Operations)
            .Distinct()
            .Select(id => new Operation { ID = id, Name = $"Op{id}" })
            .ToList();

        var authorization = new Arc4u.Security.Principal.Authorization
        {
            Operations = scopedOperations.ToList(),
            AllOperations = allOperationIds,
            Scopes = scopedOperations.Select(s => s.Scope).Distinct().ToList(),
            Roles = [new ScopedRoles { Roles = ["User"], Scope = "" }],
        };
        return new AppPrincipal(authorization, new ClaimsIdentity("TestAuth"), "S-1-0-0");
    }

    private static AuthorizationHandlerContext CreateContext(IAuthorizationRequirement requirement)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    [Fact]
    public async Task Succeeds_When_Principal_Has_All_Permissions()
    {
        var principal = CreatePrincipal(new ScopedOperations { Scope = "", Operations = [1, 2] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(1, 2);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Fails_When_Principal_Missing_One_Permission()
    {
        var principal = CreatePrincipal(new ScopedOperations { Scope = "", Operations = [1] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(1, 2);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Fails_When_Principal_Has_No_Matching_Permissions()
    {
        var principal = CreatePrincipal(new ScopedOperations { Scope = "", Operations = [99] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(1, 2);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Succeeds_With_Scoped_Permissions()
    {
        var principal = CreatePrincipal(
            new ScopedOperations { Scope = "Tenant1", Operations = [1, 2] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement("Tenant1", 1, 2);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Fails_When_Scope_Does_Not_Match()
    {
        var principal = CreatePrincipal(
            new ScopedOperations { Scope = "Tenant1", Operations = [1, 2] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement("WrongScope", 1);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Succeeds_With_Mixed_Scope_Permissions()
    {
        var principal = CreatePrincipal(
            new ScopedOperations { Scope = "ScopeA", Operations = [1] },
            new ScopedOperations { Scope = "ScopeB", Operations = [2] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(("ScopeA", 1), ("ScopeB", 2));
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Fails_With_Mixed_Scopes_When_One_Missing()
    {
        var principal = CreatePrincipal(
            new ScopedOperations { Scope = "ScopeA", Operations = [1] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(("ScopeA", 1), ("ScopeB", 2));
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Fails_When_Principal_Is_Null()
    {
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns((AppPrincipal?)null);

        var requirement = new ScopedOperationsRequirement(1);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Succeeds_With_Single_Permission()
    {
        var principal = CreatePrincipal(new ScopedOperations { Scope = "", Operations = [1] });
        var appContext = new Mock<IApplicationContext>();
        appContext.Setup(x => x.Principal).Returns(principal);

        var requirement = new ScopedOperationsRequirement(1);
        var context = CreateContext(requirement);
        var handler = new ScopedOperationsHandler(appContext.Object);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
}

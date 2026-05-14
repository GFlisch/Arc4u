using Arc4u.Authorization;
using AwesomeAssertions;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class AuthorizationOrRequirementTests
{
    [Fact]
    public void Constructor_With_Permissions_Only_Uses_Empty_Scope()
    {
        var sut = new AuthorizationOrRequirement(1, 2, 3);

        sut.Permissions.Should().BeEquivalentTo([
            (string.Empty, 1),
            (string.Empty, 2),
            (string.Empty, 3)
        ]);
    }

    [Fact]
    public void Constructor_With_Scope_Applies_Scope_To_All_Permissions()
    {
        var sut = new AuthorizationOrRequirement("Tenant1", 10, 20);

        sut.Permissions.Should().BeEquivalentTo([
            ("Tenant1", 10),
            ("Tenant1", 20)
        ]);
    }

    [Fact]
    public void Constructor_With_Tuples_Preserves_Mixed_Scopes()
    {
        var sut = new AuthorizationOrRequirement(("ScopeA", 1), ("ScopeB", 2));

        sut.Permissions.Should().BeEquivalentTo([
            ("ScopeA", 1),
            ("ScopeB", 2)
        ]);
    }

    [Fact]
    public void Constructor_With_Null_Scope_Throws_ArgumentNullException()
    {
        var act = () => new AuthorizationOrRequirement(null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_With_Empty_Permissions_Produces_Empty_Array()
    {
        var sut = new AuthorizationOrRequirement(Array.Empty<int>());

        sut.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_With_Scope_And_Empty_Permissions_Produces_Empty_Array()
    {
        var sut = new AuthorizationOrRequirement("Tenant1");

        sut.Permissions.Should().BeEmpty();
    }
}

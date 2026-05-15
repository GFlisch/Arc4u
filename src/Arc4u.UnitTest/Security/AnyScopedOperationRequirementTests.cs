using Arc4u.Authorization;
using AwesomeAssertions;
using Xunit;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class AnyScopedOperationRequirementTests
{
    [Fact]
    public void Constructor_With_Operations_Only_Uses_Empty_Scope()
    {
        var sut = new AnyScopedOperationRequirement(1, 2, 3);

        sut.Operations.Should().BeEquivalentTo([
            (string.Empty, 1),
            (string.Empty, 2),
            (string.Empty, 3)
        ]);
    }

    [Fact]
    public void Constructor_With_Scope_Applies_Scope_To_All_Operations()
    {
        var sut = new AnyScopedOperationRequirement("Tenant1", 10, 20);

        sut.Operations.Should().BeEquivalentTo([
            ("Tenant1", 10),
            ("Tenant1", 20)
        ]);
    }

    [Fact]
    public void Constructor_With_Tuples_Preserves_Mixed_Scopes()
    {
        var sut = new AnyScopedOperationRequirement(("ScopeA", 1), ("ScopeB", 2));

        sut.Operations.Should().BeEquivalentTo([
            ("ScopeA", 1),
            ("ScopeB", 2)
        ]);
    }

    [Fact]
    public void Constructor_With_Null_Scope_Throws_ArgumentNullException()
    {
        var act = () => new AnyScopedOperationRequirement(null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_With_Empty_Operations_Produces_Empty_Array()
    {
        var sut = new AnyScopedOperationRequirement(Array.Empty<int>());

        sut.Operations.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_With_Scope_And_Empty_Operations_Produces_Empty_Array()
    {
        var sut = new AnyScopedOperationRequirement("Tenant1");

        sut.Operations.Should().BeEmpty();
    }
}

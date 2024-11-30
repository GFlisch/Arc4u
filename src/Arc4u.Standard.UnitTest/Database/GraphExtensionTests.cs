using System.Linq.Expressions;
using Arc4u.EfCore;
using Xunit;

namespace Arc4u.Standard.UnitTest.Database;

public class GraphExtensionTests
{
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public TestEntity RelatedEntity { get; set; } = default!;
        public ICollection<TestEntity> RelatedEntities { get; set; } = [];
    }

    [Fact]
    public void ApplySingleReferences_ShouldIncludeSingleReferences()
    {
        // Arrange
        var graph = new Graph<TestEntity>(new List<string> { "RelatedEntity" });
        var queryable = new List<TestEntity>().AsQueryable();

        // Act
        var result = graph.ApplySingleReferences(queryable);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ApplySetReferences_ShouldIncludeAllReferences()
    {
        // Arrange
        var graph = new Graph<TestEntity>(new List<string> { "RelatedEntity", "RelatedEntities" });
        var queryable = new List<TestEntity>().AsQueryable();

        // Act
        var result = graph.ApplySetReferences(queryable);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ApplyReferences_ShouldThrowExceptionForMultiLevelReference()
    {
        // Arrange
        var graph = new Graph<TestEntity>(new List<string> { "RelatedEntity.RelatedEntities" });
        var queryable = new List<TestEntity>().AsQueryable();
        Expression<Func<TestEntity, ICollection<TestEntity>>> path = e => e.RelatedEntities;

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => graph.ApplyReferences(queryable, path));
        Assert.Equal("It is not allowed to check more than one level!", exception.Message);
    }
}

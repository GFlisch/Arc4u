using System.Collections.Generic;
using Arc4u.Core;
using AutoFixture.AutoMoq;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Arc4u.UnitTest.Core;

[Trait("Category", "CI")]
public class ValueObjectTests
{
    public ValueObjectTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void Test_Null_Operator_Should()
    {
        SimpleObject? o = null;

        (o == null).Should().BeTrue();
    }

    [Fact]
    public void Test_Null_Equal_Should()
    {
        SimpleObject? o1 = null;
        SimpleObject? o2 = null;

        (o1 == o2).Should().BeTrue();
    }

    [Fact]
    public void Test_Null_NotEqual_Operator_Should()
    {
        SimpleObject? o = _fixture.Create<SimpleObject>();

        (o != null).Should().BeTrue();
    }

    [Fact]
    public void Test_Null_NotEqual_Should()
    {
        SimpleObject? o1 = _fixture.Create<SimpleObject>();
        SimpleObject? o2 = _fixture.Create<SimpleObject>();

        (o1 != o2).Should().BeTrue();
    }

    [Fact]
    public void Test_Null_NotOperator1_Should()
    {
        SimpleObject? o = null;

        (o != null).Should().BeFalse();
    }

    [Fact]
    public void Test_Null_NotOperator2_Should()
    {
        SimpleObject? o1 = null;
        SimpleObject? o2 = null;

        (o1 != o2).Should().BeFalse();
    }

}

public class  SimpleObject : ValueObject
{
    public int Counter { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Counter;
    }
}

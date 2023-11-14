using AutoFixture.AutoMoq;
using AutoFixture;
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

    //[Fact]
    //public Test_IsAuthorise_By_Int_Should()
    //{

    //}
}

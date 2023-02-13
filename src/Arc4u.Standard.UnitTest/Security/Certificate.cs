using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;

namespace Arc4u.Standard.UnitTest.Security;

[Trait("Category", "CI")]
public class Certificate
{
    public Certificate()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;



}

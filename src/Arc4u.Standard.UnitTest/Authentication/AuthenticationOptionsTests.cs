using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;

namespace Arc4u.Standard.UnitTest.Authentication;

[Trait("Category", "CI")]
public class AuthenticationOptionsTests
{
    public AuthenticationOptionsTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void testshould()
    {

    }
}

using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Arc4u.Security.Cryptography;
using FluentAssertions;
using System.Security.Cryptography.X509Certificates;
using System.Net.WebSockets;

namespace Arc4u.UnitTest.Security;

[Trait("Category", "CI")]
public class CertificateTests
{
    public CertificateTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void FileCertificateShouldBe()
    {
        //arrange
        var publicCert = @".\Configs\cert.pem";
        var privateCert = @".\Configs\key.pem";
        var plainText = "FileCertificateShouldBe()";

        // act
        var certificate = X509Certificate2.CreateFromPemFile(publicCert, privateCert);
        var cypherText = certificate.Encrypt(plainText);
        var sut = certificate.Decrypt(cypherText);


        // assert
        certificate.Should().NotBeNull();
        sut.Should().Be(plainText);
    }
}

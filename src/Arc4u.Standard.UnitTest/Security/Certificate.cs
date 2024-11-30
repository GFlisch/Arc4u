using System.Security.Cryptography.X509Certificates;
using Arc4u.Security.Cryptography;
using Arc4u.UnitTest.Decryptor;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Xunit;

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

    [Theory]
    [InlineData("0123456789")]
    [InlineData("012345678901234567890123456789012345678901234567890123456789")]
    [InlineData("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789")]
    public void Small_Text_Shoud_Directly_Encrypted(string plainText)
    {
        //arranges
        var certificate = CertificateDecryptor.GetX509Certificate2();

        // act
        var cypherText = certificate.Encrypt(plainText);
        var sut = certificate.Decrypt(cypherText);

        // assert
        certificate.Should().NotBeNull();
        cypherText.Should().NotContain(".");
        sut.Should().Be(plainText);
    }

    [Fact]
    public void Large_Text_Shoud_Encrypted_With_Aes()
    {
        //arranges
        var certificate = CertificateDecryptor.GetX509Certificate2();
        var plainText = new string('A', 600);
        // act
        var cypherText = certificate.Encrypt(plainText);
        var sut = certificate.Decrypt(cypherText);

        // assert
        certificate.Should().NotBeNull();
        cypherText.Should().Contain(".");
        sut.Should().Be(plainText);
    }
}

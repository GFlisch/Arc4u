using System.Collections.Generic;
using AutoFixture.AutoMoq;
using AutoFixture;
using Xunit;
using Microsoft.Extensions.Configuration;
using Arc4u.Security.Cryptography;
using FluentAssertions;
using System.Security.Cryptography.X509Certificates;
using Moq;

namespace Arc4u.UnitTest.Decryptor;

[Trait("Category", "CI")]
public class CertificateLoader
{
    public CertificateLoader()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
    }

    private readonly Fixture _fixture;

    [Fact]
    public void FileCertificateShould()
    {
        var certificate = CertificateDecryptor.GetX509Certificate2();

        var plainText = _fixture.Create<string>();
        var cypherText = certificate.Encrypt(plainText);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["EncryptionCertificate:File:Cert"] = @".\Configs\cert.pem",
                    ["EncryptionCertificate:File:Key"] = @".\Configs\key.pem",
                }).Build();

        var sut = _fixture.Create<X509CertificateLoader>();

        // act
        var configCert = sut.FindCertificate(config, "EncryptionCertificate");

        // assert
        configCert.Should().NotBeNull();
        plainText.Should().Be(configCert.Decrypt(cypherText));
    }

    [Fact]
    public void FileCertificateNotPresentShould()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["EncryptionCertificate:File:Cert"] = @".\cert.pem",
                    ["EncryptionCertificate:File:Key"] = @".\key.pem",
                }).Build();


        var sut = _fixture.Create<X509CertificateLoader>();

        // act
        var configCert = sut.FindCertificate(config, "EncryptionCertificate");

        // assert
        configCert.Should().BeNull();
    }

    [Fact]
    public void CertificateNotPresentShould()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["EncryptionCertificate:Store:Name"] = "friendlyName"
                }).Build();

        var sut = _fixture.Create<X509CertificateLoader>();

        // act
        var configCert = Record.Exception(() => sut.FindCertificate(config, "EncryptionCertificate"));

        // assert
        configCert.Should().NotBeNull();
        configCert.Should().BeOfType<KeyNotFoundException>();
    }

}

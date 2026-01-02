using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AwesomeAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class MongoClientExtensionsTests
{
    [Fact]
    public void SetTls_WithNullConfig_ShouldNotModifySettings()
    {
        var settings = new MongoClientSettings();
        var mockClient = new Mock<IMongoClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.Settings).Returns(settings);

        var result = mockClient.Object.SetTls(null);

        result.Should().BeSameAs(mockClient.Object);
        settings.UseTls.Should().BeFalse();
    }

    [Fact]
    public void SetTls_WithConfig_ShouldEnableTlsAndSetSslSettings()
    {
        var settings = new MongoClientSettings();
        var mockClient = new Mock<IMongoClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.Settings).Returns(settings);
        var sslSettings = new SslSettings();

        var result = mockClient.Object.SetTls(sslSettings);

        result.Should().BeSameAs(mockClient.Object);
        settings.UseTls.Should().BeTrue();
        settings.SslSettings.Should().BeSameAs(sslSettings);
    }

    [Fact]
    public void SetSsh_WithNullConfig_ShouldNotModifySettings()
    {
        var settings = new MongoClientSettings();
        var mockClient = new Mock<IMongoClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.Settings).Returns(settings);

        var result = mockClient.Object.SetSsh(null);

        result.Should().BeSameAs(mockClient.Object);
    }

    [Fact]
    public void SetSsh_WithConfig_ShouldSetServerAddress()
    {
        var settings = new MongoClientSettings();
        var mockClient = new Mock<IMongoClient>(MockBehavior.Strict);
        mockClient.Setup(c => c.Settings).Returns(settings);
        using var forwardedPort = new Renci.SshNet.ForwardedPortLocal("127.0.0.1", 27018, "remoteHost", 27017);
        var mockSshClient = new Mock<Renci.SshNet.SshClient>(MockBehavior.Strict, "host", "user", "pass");
        var sshConfig = new SshConfig(mockSshClient.Object, forwardedPort, 27018, "127.0.0.1");

        var result = mockClient.Object.SetSsh(sshConfig);

        result.Should().BeSameAs(mockClient.Object);
        settings.Server.Host.Should().Be("127.0.0.1");
        settings.Server.Port.Should().Be(27018);
    }
}

using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AwesomeAssertions;
using Moq;
using Renci.SshNet;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class SshConfigTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        using var forwardedPort = new ForwardedPortLocal("127.0.0.1", 27017, "remoteHost", 27017);
        var mockSshClient = new Mock<SshClient>(MockBehavior.Strict, "host", "username", "password");
        const uint boundPort = 27017;
        const string boundHost = "127.0.0.1";

        var config = new SshConfig(
            mockSshClient.Object,
            forwardedPort,
            boundPort,
            boundHost);

        config.SshClient.Should().BeSameAs(mockSshClient.Object);
        config.ForwardedPortLocal.Should().BeSameAs(forwardedPort);
        config.BoundPort.Should().Be(boundPort);
        config.BoundHost.Should().Be(boundHost);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        var type = typeof(SshConfig);
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        type.GetProperty(nameof(SshConfig.SshClient), flags)!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(SshConfig.ForwardedPortLocal), flags)!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(SshConfig.BoundPort), flags)!.CanWrite.Should().BeFalse();
        type.GetProperty(nameof(SshConfig.BoundHost), flags)!.CanWrite.Should().BeFalse();
    }
}

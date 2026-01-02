using AdaskoTheBeAsT.MongoDbMigrations.Document;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Document;

public class ServerAddressConfigTests
{
    [Fact]
    public void Host_WithValidValue_ShouldSetHost()
    {
        var config = new ServerAddressConfig { Host = "localhost" };

        config.Host.Should().Be("localhost");
    }

    [Fact]
    public void Host_WithNull_ShouldThrowArgumentNullException()
    {
        var config = new ServerAddressConfig();

        var act = () => config.Host = null!;

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Host_WithEmptyString_ShouldThrowArgumentNullException()
    {
        var config = new ServerAddressConfig();

        var act = () => config.Host = string.Empty;

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Port_WithValidValue_ShouldSetPort()
    {
        var config = new ServerAddressConfig { Port = 27017 };

        config.Port.Should().Be(27017u);
    }

    [Fact]
    public void Port_WithMaxValidValue_ShouldSetPort()
    {
        var config = new ServerAddressConfig { Port = 65535 };

        config.Port.Should().Be(65535u);
    }

    [Fact]
    public void Port_WithValueGreaterThan65535_ShouldThrowArgumentOutOfRangeException()
    {
        var config = new ServerAddressConfig();

        var act = () => config.Port = 65536;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PortAsInt_ShouldReturnPortAsInteger()
    {
        var config = new ServerAddressConfig { Port = 27017 };

        config.PortAsInt.Should().Be(27017);
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new ServerAddressConfig();

        config.Host.Should().Be(string.Empty);
        config.Port.Should().Be(0u);
        config.PortAsInt.Should().Be(0);
    }
}

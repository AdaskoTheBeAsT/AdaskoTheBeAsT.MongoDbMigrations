using AdaskoTheBeAsT.MongoDbMigrations.Exceptions;
using AwesomeAssertions;
using Xunit;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void DatabaseOutdatedException_DefaultConstructor_ShouldCreate()
    {
        var ex = new DatabaseOutdatedException();

        ex.Should().NotBeNull();
        ex.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DatabaseOutdatedException_WithMessage_ShouldSetMessage()
    {
        const string message = "Test message";

        var ex = new DatabaseOutdatedException(message);

        ex.Message.Should().Be(message);
    }

    [Fact]
    public void DatabaseOutdatedException_WithMessageAndInnerException_ShouldSetBoth()
    {
        const string message = "Test message";
        var inner = new InvalidOperationException("Inner");

        var ex = new DatabaseOutdatedException(message, inner);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DatabaseOutdatedException_WithVersions_ShouldFormatMessage()
    {
        var dbVersion = new Version(1, 0, 0);
        var targetVersion = new Version(2, 0, 0);

        var ex = new DatabaseOutdatedException(dbVersion, targetVersion);

        ex.Message.Should().Contain("1.0.0");
        ex.Message.Should().Contain("2.0.0");
    }

    [Fact]
    public void InvalidVersionException_DefaultConstructor_ShouldCreate()
    {
        var ex = new InvalidVersionException();

        ex.Should().NotBeNull();
    }

    [Fact]
    public void InvalidVersionException_WithMessage_ShouldSetMessage()
    {
        const string message = "Invalid version";
        const string expectedMessage = "Invalid value: Invalid version";

        var ex = new InvalidVersionException(message);

        ex.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void InvalidVersionException_WithMessageAndInnerException_ShouldSetBoth()
    {
        const string message = "Invalid version";
        var inner = new FormatException("Format error");

        var ex = new InvalidVersionException(message, inner);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void MigrationNotFoundException_DefaultConstructor_ShouldCreate()
    {
        var ex = new MigrationNotFoundException();

        ex.Should().NotBeNull();
    }

    [Fact]
    public void MigrationNotFoundException_WithMessage_ShouldSetMessage()
    {
        const string message = "Migration not found";

        var ex = new MigrationNotFoundException(message);

        ex.Message.Should().Be(message);
    }

    [Fact]
    public void MigrationNotFoundException_WithMessageAndInnerException_ShouldSetBoth()
    {
        const string assemblyName = "TestAssembly";
        const string message = "Migrations are not found in assembly TestAssembly";
        var inner = new InvalidOperationException("Inner");

        var ex = new MigrationNotFoundException(assemblyName, inner);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void VersionStringTooLongException_DefaultConstructor_ShouldCreate()
    {
        var ex = new VersionStringTooLongException();

        ex.Should().NotBeNull();
    }

    [Fact]
    public void VersionStringTooLongException_WithMessage_ShouldSetMessage()
    {
        const string version = "11111.111.111";
        const string expectedMessage = "Versions must have the format: major.minor.revision, this doesn't match: 11111.111.111";

        var ex = new VersionStringTooLongException(version);

        ex.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void VersionStringTooLongException_WithMessageAndInnerException_ShouldSetBoth()
    {
        const string message = "Version string too long";
        var inner = new FormatException("Format error");

        var ex = new VersionStringTooLongException(message, inner);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().Be(inner);
    }
}

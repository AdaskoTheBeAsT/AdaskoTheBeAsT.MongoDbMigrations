using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class MigrationDescriptorTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        var version = new Version(1, 0, 0);
        var upCollections = new[] { "users", "orders" };
        var downCollections = new[] { "users" };

        var descriptor = new MigrationDescriptor(
            typeof(TestMigration),
            version,
            "Test Migration",
            upCollections,
            downCollections);

        descriptor.MigrationType.Should().Be(typeof(TestMigration));
        descriptor.Version.Should().Be(version);
        descriptor.Name.Should().Be("Test Migration");
        descriptor.UpCollections.Should().BeEquivalentTo(upCollections);
        descriptor.DownCollections.Should().BeEquivalentTo(downCollections);
    }

    [Fact]
    public void Constructor_WithNullMigrationType_ShouldThrowArgumentNullException()
    {
        var act = () => new MigrationDescriptor(
            null!,
            new Version(1, 0, 0),
            "TestName",
            Array.Empty<string>(),
            Array.Empty<string>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("migrationType");
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        var act = () => new MigrationDescriptor(
            typeof(TestMigration),
            new Version(1, 0, 0),
            null!,
            Array.Empty<string>(),
            Array.Empty<string>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNullUpCollections_ShouldThrowArgumentNullException()
    {
        var act = () => new MigrationDescriptor(
            typeof(TestMigration),
            new Version(1, 0, 0),
            "TestName",
            null!,
            Array.Empty<string>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("upCollections");
    }

    [Fact]
    public void Constructor_WithNullDownCollections_ShouldThrowArgumentNullException()
    {
        var act = () => new MigrationDescriptor(
            typeof(TestMigration),
            new Version(1, 0, 0),
            "TestName",
            Array.Empty<string>(),
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("downCollections");
    }

    [Fact]
    public void CreateInstance_ShouldReturnNewMigrationInstance()
    {
        var descriptor = new MigrationDescriptor(
            typeof(TestMigration),
            new Version(1, 0, 0),
            "TestName",
            Array.Empty<string>(),
            Array.Empty<string>());

        var instance = descriptor.CreateInstance();

        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestMigration>();
    }

    [Fact]
    public void CreateInstance_ShouldReturnNewInstanceEachTime()
    {
        var descriptor = new MigrationDescriptor(
            typeof(TestMigration),
            new Version(1, 0, 0),
            "TestName",
            Array.Empty<string>(),
            Array.Empty<string>());

        var instance1 = descriptor.CreateInstance();
        var instance2 = descriptor.CreateInstance();

        instance1.Should().NotBeSameAs(instance2);
    }

    private sealed class TestMigration : IMigration
    {
        public Version Version => new(1, 0, 0);

        public string Name => "Test Migration";

        public Task UpAsync(MigrationContext context) => Task.CompletedTask;

        public Task DownAsync(MigrationContext context) => Task.CompletedTask;
    }
}

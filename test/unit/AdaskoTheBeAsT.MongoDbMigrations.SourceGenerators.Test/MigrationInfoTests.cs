using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators.Test;

public class MigrationInfoTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var version = new VersionInfo(1, 2, 3);
        var upCollections = new List<string> { "users", "orders" };
        var downCollections = new List<string> { "users" };

        var migrationInfo = new MigrationInfo(
            "TestApp.TestMigration",
            version,
            "Test Migration",
            upCollections,
            downCollections);

        migrationInfo.FullTypeName.Should().Be("TestApp.TestMigration");
        migrationInfo.Version.Should().Be(version);
        migrationInfo.Name.Should().Be("Test Migration");
        migrationInfo.UpCollections.Should().BeEquivalentTo(upCollections);
        migrationInfo.DownCollections.Should().BeEquivalentTo(downCollections);
    }

    [Fact]
    public void Constructor_WithEmptyCollections_ShouldWork()
    {
        var version = new VersionInfo(1, 0, 0);

        var migrationInfo = new MigrationInfo(
            "TestApp.Migration",
            version,
            "Migration",
            new List<string>(),
            new List<string>());

        migrationInfo.UpCollections.Should().BeEmpty();
        migrationInfo.DownCollections.Should().BeEmpty();
    }
}

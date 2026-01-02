using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AwesomeAssertions;
using Xunit;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class InterimMigrationResultTests
{
    [Fact]
    public void InterimMigrationResult_DefaultValues_ShouldBeCorrect()
    {
        var result = new InterimMigrationResult();

        result.MigrationName.Should().BeNull();
        result.TargetVersion.Should().Be(Version.Zero());
        result.ServerAddress.Should().BeNull();
        result.DatabaseName.Should().BeNull();
        result.CurrentNumber.Should().Be(0);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void InterimMigrationResult_AllPropertiesSettable()
    {
        var version = new Version(1, 0, 0);
        var result = new InterimMigrationResult
        {
            MigrationName = "Migration 1",
            TargetVersion = version,
            ServerAddress = "localhost",
            DatabaseName = "testdb",
            CurrentNumber = 1,
            TotalCount = 5,
        };

        result.MigrationName.Should().Be("Migration 1");
        result.TargetVersion.Should().Be(version);
        result.ServerAddress.Should().Be("localhost");
        result.DatabaseName.Should().Be("testdb");
        result.CurrentNumber.Should().Be(1);
        result.TotalCount.Should().Be(5);
    }
}

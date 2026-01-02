using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AwesomeAssertions;
using Xunit;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class MigrationResultTests
{
    [Fact]
    public void MigrationResult_DefaultValues_ShouldBeCorrect()
    {
        var result = new MigrationResult();

        result.CurrentVersion.Should().Be(Version.Zero());
        result.InterimSteps.Should().NotBeNull();
        result.InterimSteps.Should().BeEmpty();
        result.ServerAddress.Should().BeNull();
        result.DatabaseName.Should().BeNull();
        result.Success.Should().BeFalse();
        result.IsDryRun.Should().BeFalse();
    }

    [Fact]
    public void MigrationResult_InterimSteps_ShouldBeModifiable()
    {
        var result = new MigrationResult();
        var step = new InterimMigrationResult
        {
            MigrationName = "Test Migration",
            TargetVersion = new Version(1, 0, 0),
            CurrentNumber = 1,
            TotalCount = 3,
        };

        result.InterimSteps.Add(step);

        result.InterimSteps.Should().HaveCount(1);
        result.InterimSteps[0].MigrationName.Should().Be("Test Migration");
    }

    [Fact]
    public void MigrationResult_AllPropertiesSettable()
    {
        var version = new Version(1, 2, 3);
        var result = new MigrationResult
        {
            CurrentVersion = version,
            ServerAddress = "localhost:27017",
            DatabaseName = "testdb",
            Success = true,
            IsDryRun = true,
        };

        result.CurrentVersion.Should().Be(version);
        result.ServerAddress.Should().Be("localhost:27017");
        result.DatabaseName.Should().Be("testdb");
        result.Success.Should().BeTrue();
        result.IsDryRun.Should().BeTrue();
    }
}

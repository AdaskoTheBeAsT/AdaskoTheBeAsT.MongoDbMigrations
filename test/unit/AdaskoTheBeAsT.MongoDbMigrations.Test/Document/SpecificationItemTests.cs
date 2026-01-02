using AdaskoTheBeAsT.MongoDbMigrations.Document;
using AwesomeAssertions;
using Xunit;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Document;

public class SpecificationItemTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var item = new SpecificationItem();

        item.Id.Should().Be(string.Empty);
        item.Name.Should().Be(string.Empty);
        item.Ver.Should().Be(Version.Zero());
        item.IsUp.Should().BeFalse();
        item.ApplyingDateTime.Should().Be(default);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        var now = DateTime.UtcNow;
        var version = new Version(1, 2, 3);

        var item = new SpecificationItem
        {
            Id = "test-id",
            Name = "Test Migration",
            Ver = version,
            IsUp = true,
            ApplyingDateTime = now,
        };

        item.Id.Should().Be("test-id");
        item.Name.Should().Be("Test Migration");
        item.Ver.Should().Be(version);
        item.IsUp.Should().BeTrue();
        item.ApplyingDateTime.Should().Be(now);
    }
}

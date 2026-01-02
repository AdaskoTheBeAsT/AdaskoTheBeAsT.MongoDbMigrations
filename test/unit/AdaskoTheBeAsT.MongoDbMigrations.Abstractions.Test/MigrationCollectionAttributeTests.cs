using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class MigrationCollectionAttributeTests
{
    [Fact]
    public void Constructor_WithCollectionName_ShouldSetDefaultDirection()
    {
        var attribute = new MigrationCollectionAttribute("users");

        attribute.CollectionName.Should().Be("users");
        attribute.Direction.Should().Be(MigrationDirection.Both);
    }

    [Fact]
    public void Constructor_WithCollectionNameAndDirection_ShouldSetBothProperties()
    {
        var attribute = new MigrationCollectionAttribute("orders", MigrationDirection.Up);

        attribute.CollectionName.Should().Be("orders");
        attribute.Direction.Should().Be(MigrationDirection.Up);
    }

    [Theory]
    [InlineData(MigrationDirection.Up)]
    [InlineData(MigrationDirection.Down)]
    [InlineData(MigrationDirection.Both)]
    public void Constructor_WithDifferentDirections_ShouldSetCorrectly(MigrationDirection direction)
    {
        var attribute = new MigrationCollectionAttribute("test", direction);

        attribute.Direction.Should().Be(direction);
    }

    [Fact]
    public void Attribute_ShouldAllowMultiple()
    {
        var attributes = typeof(TestMigrationWithMultipleCollections)
            .GetCustomAttributes(typeof(MigrationCollectionAttribute), false);

        attributes.Should().HaveCount(2);
    }

    [MigrationCollection("users", MigrationDirection.Up)]
    [MigrationCollection("orders", MigrationDirection.Down)]
    private sealed class TestMigrationWithMultipleCollections
    {
    }
}

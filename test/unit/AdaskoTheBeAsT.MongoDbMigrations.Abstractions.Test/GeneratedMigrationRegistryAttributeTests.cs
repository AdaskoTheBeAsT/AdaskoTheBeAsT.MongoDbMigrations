using System.Reflection;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class GeneratedMigrationRegistryAttributeTests
{
    [Fact]
    public void GeneratedMigrationRegistryAttribute_ShouldBeCreatable()
    {
        var attribute = new GeneratedMigrationRegistryAttribute();

        attribute.Should().NotBeNull();
    }

    [Fact]
    public void GeneratedMigrationRegistryAttribute_ShouldBeApplicableToClass()
    {
        var attributes = typeof(GeneratedRegistry)
            .GetCustomAttributes(typeof(GeneratedMigrationRegistryAttribute), false);

        attributes.Should().HaveCount(1);
    }

    [Fact]
    public void GeneratedMigrationRegistryAttribute_ShouldNotAllowMultiple()
    {
        var attributeUsage = typeof(GeneratedMigrationRegistryAttribute).GetCustomAttributes<AttributeUsageAttribute>()
            .ToList();

        attributeUsage.Should().NotBeNull();
        attributeUsage[0].AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void GeneratedMigrationRegistryAttribute_ShouldTargetClassOnly()
    {
        var attributeUsage = typeof(GeneratedMigrationRegistryAttribute).GetCustomAttributes<AttributeUsageAttribute>()
            .ToList();

        attributeUsage.Should().NotBeNull();
        attributeUsage[0].ValidOn.Should().Be(AttributeTargets.Class);
    }

    [GeneratedMigrationRegistry]
    private static class GeneratedRegistry
    {
    }
}

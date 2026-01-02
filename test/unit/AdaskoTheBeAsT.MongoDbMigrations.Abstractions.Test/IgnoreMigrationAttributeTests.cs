using System.Reflection;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class IgnoreMigrationAttributeTests
{
    [Fact]
    public void IgnoreMigrationAttribute_ShouldBeCreatable()
    {
        var attribute = new IgnoreMigrationAttribute();

        attribute.Should().NotBeNull();
    }

    [Fact]
    public void IgnoreMigrationAttribute_ShouldBeApplicableToClass()
    {
        var attributes = typeof(IgnoredMigration)
            .GetCustomAttributes(typeof(IgnoreMigrationAttribute), false);

        attributes.Should().HaveCount(1);
    }

    [Fact]
    public void IgnoreMigrationAttribute_ShouldNotAllowMultiple()
    {
        var attributeUsage = typeof(IgnoreMigrationAttribute).GetCustomAttributes<AttributeUsageAttribute>()
            .ToList();

        attributeUsage.Should().NotBeNull();
        attributeUsage[0].AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void IgnoreMigrationAttribute_ShouldTargetClassOnly()
    {
        var attributeUsage = typeof(IgnoreMigrationAttribute).GetCustomAttributes<AttributeUsageAttribute>()
            .ToList();

        attributeUsage.Should().NotBeNull();
        attributeUsage[0].ValidOn.Should().Be(AttributeTargets.Class);
    }

    [IgnoreMigration]
    private sealed class IgnoredMigration
    {
    }
}

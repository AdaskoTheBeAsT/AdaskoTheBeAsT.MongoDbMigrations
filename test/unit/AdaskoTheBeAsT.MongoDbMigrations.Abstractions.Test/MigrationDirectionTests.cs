using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class MigrationDirectionTests
{
    [Fact]
    public void MigrationDirection_ShouldHaveUpValue()
    {
        MigrationDirection.Up.Should().BeDefined();
        ((int)MigrationDirection.Up).Should().Be(0);
    }

    [Fact]
    public void MigrationDirection_ShouldHaveDownValue()
    {
        MigrationDirection.Down.Should().BeDefined();
        ((int)MigrationDirection.Down).Should().Be(1);
    }

    [Fact]
    public void MigrationDirection_ShouldHaveBothValue()
    {
        MigrationDirection.Both.Should().BeDefined();
        ((int)MigrationDirection.Both).Should().Be(2);
    }

    [Fact]
    public void MigrationDirection_ShouldHaveThreeValues()
    {
#if NET8_0_OR_GREATER
        var values = Enum.GetValues<MigrationDirection>();
        values.Should().HaveCount(3);
#endif
#if NET472_OR_GREATER
        var values = Enum.GetValues(typeof(MigrationDirection));
        values.Length.Should().Be(3);
#endif
    }
}

using AdaskoTheBeAsT.MongoDbMigrations.Document;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Document;

public class MongoEmulationTests
{
    [Fact]
    public void MongoEmulation_None_ShouldHaveValueZero()
    {
        ((int)MongoEmulation.None).Should().Be(0);
    }

    [Fact]
    public void MongoEmulation_AzureCosmos_ShouldHaveValueOne()
    {
        ((int)MongoEmulation.AzureCosmos).Should().Be(1);
    }

    [Fact]
    public void MongoEmulation_AwsDocument_ShouldHaveValueTwo()
    {
        ((int)MongoEmulation.AwsDocument).Should().Be(2);
    }

    [Theory]
    [InlineData(MongoEmulation.None)]
    [InlineData(MongoEmulation.AzureCosmos)]
    [InlineData(MongoEmulation.AwsDocument)]
    public void MongoEmulation_AllValues_ShouldBeDefined(MongoEmulation emulation)
    {
        System.Enum.IsDefined(typeof(MongoEmulation), emulation).Should().BeTrue();
    }
}

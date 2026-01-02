using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class SchemeValidationResultTests
{
    [Fact]
    public void SchemeValidationResult_InitialState_ShouldBeEmpty()
    {
        var result = new SchemeValidationResult();

        result.ValidCollections.Should().BeEmpty();
        result.FailedCollections.Should().BeEmpty();
    }

    [Fact]
    public void SchemeValidationResult_AddValidCollection_ShouldAddToValid()
    {
        var result = new SchemeValidationResult();

        result.Add("users", isFailed: false);

        result.ValidCollections.Should().Contain("users");
        result.FailedCollections.Should().BeEmpty();
    }

    [Fact]
    public void SchemeValidationResult_AddFailedCollection_ShouldAddToFailed()
    {
        var result = new SchemeValidationResult();

        result.Add("orders", isFailed: true);

        result.FailedCollections.Should().Contain("orders");
        result.ValidCollections.Should().BeEmpty();
    }

    [Fact]
    public void SchemeValidationResult_AddDuplicates_ShouldDistinct()
    {
        var result = new SchemeValidationResult();

        result.Add("users", isFailed: false);
        result.Add("users", isFailed: false);

        result.ValidCollections.Should().HaveCount(1);
    }
}

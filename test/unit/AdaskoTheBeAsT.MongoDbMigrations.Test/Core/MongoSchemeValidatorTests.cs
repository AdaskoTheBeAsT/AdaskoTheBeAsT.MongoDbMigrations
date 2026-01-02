using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AwesomeAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class MongoSchemeValidatorTests
{
    [Fact]
    public Task ValidateAsync_WithNullCollectionNames_ShouldThrowArgumentNullException()
    {
        var databaseMock = new Mock<IMongoDatabase>(MockBehavior.Loose);

        var act = () => MongoSchemeValidator.ValidateAsync(null!, databaseMock.Object);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public Task ValidateAsync_WithNullDatabase_ShouldThrowArgumentNullException()
    {
        var collectionNames = new List<string>();

        var act = () => MongoSchemeValidator.ValidateAsync(collectionNames, null!);

        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyCollectionNames_ShouldReturnEmptyResult()
    {
        var databaseMock = new Mock<IMongoDatabase>(MockBehavior.Loose);
        var collectionNames = new List<string>();

        var result = await MongoSchemeValidator.ValidateAsync(collectionNames, databaseMock.Object);

        result.Should().NotBeNull();
        result.ValidCollections.Should().BeEmpty();
        result.FailedCollections.Should().BeEmpty();
    }
}

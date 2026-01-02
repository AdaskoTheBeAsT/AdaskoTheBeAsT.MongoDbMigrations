using AwesomeAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class MigrationContextTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        var databaseMock = new Mock<IMongoDatabase>(MockBehavior.Loose);
        var sessionMock = new Mock<IClientSessionHandle>(MockBehavior.Loose);
        using var cts = new CancellationTokenSource();

        var context = new MigrationContext(databaseMock.Object, sessionMock.Object, cts.Token);

        context.Database.Should().BeSameAs(databaseMock.Object);
        context.Session.Should().BeSameAs(sessionMock.Object);
        context.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public void Constructor_WithNullSession_ShouldSetSessionToNull()
    {
        var databaseMock = new Mock<IMongoDatabase>(MockBehavior.Loose);

        var context = new MigrationContext(databaseMock.Object, null, CancellationToken.None);

        context.Database.Should().BeSameAs(databaseMock.Object);
        context.Session.Should().BeNull();
        context.CancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public void Constructor_WithDefaultCancellationToken_ShouldWork()
    {
        var databaseMock = new Mock<IMongoDatabase>(MockBehavior.Loose);

        var context = new MigrationContext(databaseMock.Object, null, default);

        context.CancellationToken.Should().Be(CancellationToken.None);
    }
}

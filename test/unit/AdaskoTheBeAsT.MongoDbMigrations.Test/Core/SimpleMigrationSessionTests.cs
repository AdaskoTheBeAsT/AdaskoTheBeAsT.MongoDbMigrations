using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Core;

public class SimpleMigrationSessionTests
{
    [Fact]
    public void BeginTransaction_ShouldReturnSimpleTransaction()
    {
        var session = new SimpleMigrationSession();

        using var transaction = session.BeginTransaction();

        transaction.Should().NotBeNull();
    }

    [Fact]
    public void SimpleTransaction_Session_ShouldBeNull()
    {
        var session = new SimpleMigrationSession();

        using var transaction = session.BeginTransaction();

        transaction.Session.Should().BeNull();
    }

    [Fact]
    public void SimpleTransaction_Commit_ShouldNotThrow()
    {
        var session = new SimpleMigrationSession();

        using var transaction = session.BeginTransaction();

        var act = () => transaction.Commit();

        act.Should().NotThrow();
    }

    [Fact]
    public void SimpleTransaction_CommitWithCancellationToken_ShouldNotThrow()
    {
        var session = new SimpleMigrationSession();
        using var cts = new CancellationTokenSource();

        using var transaction = session.BeginTransaction();

        var act = () => transaction.Commit(cts.Token);

        act.Should().NotThrow();
    }

    [Fact]
    public void SimpleTransaction_Dispose_ShouldNotThrow()
    {
        var session = new SimpleMigrationSession();

        var transaction = session.BeginTransaction();

        var act = () => transaction.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void SimpleTransaction_MultipleDisposes_ShouldNotThrow()
    {
        var session = new SimpleMigrationSession();

        using (var transaction = session.BeginTransaction())
        {
#pragma warning disable IDISP016 // Don't use disposed instance
            transaction.Dispose();
#pragma warning restore IDISP016 // Don't use disposed instance
            transaction.Session.Should().BeNull();
        }
    }
}

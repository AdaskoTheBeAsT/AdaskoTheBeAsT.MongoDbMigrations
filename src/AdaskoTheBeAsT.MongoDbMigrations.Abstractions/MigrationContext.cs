using System.Threading;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Context passed to migration Up/Down methods.
/// </summary>
public class MigrationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationContext"/> class.
    /// </summary>
    /// <param name="database">The MongoDB database.</param>
    /// <param name="session">Optional client session for transactions.</param>
    /// <param name="token">Cancellation token.</param>
    public MigrationContext(
        IMongoDatabase database,
        IClientSessionHandle? session,
        CancellationToken token)
    {
        Database = database;
        Session = session;
        CancellationToken = token;
    }

    /// <summary>
    /// Gets the MongoDB database.
    /// </summary>
    public IMongoDatabase Database { get; }

    /// <summary>
    /// Gets the client session for transaction support.
    /// </summary>
    public IClientSessionHandle? Session { get; }

    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}

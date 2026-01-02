using System.Threading;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// MongoDB migration session with transaction support.
/// </summary>
public sealed class MongoMigrationSession : IMigrationSession
{
    private readonly IMongoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoMigrationSession"/> class.
    /// </summary>
    /// <param name="client">MongoDB client.</param>
    public MongoMigrationSession(IMongoClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public IMigrationTransaction BeginTransaction() => new MongoMigrationTransaction(_client.StartSession());

    private sealed class MongoMigrationTransaction : IMigrationTransaction
    {
        private readonly IClientSessionHandle _session;

        public MongoMigrationTransaction(IClientSessionHandle session)
        {
            _session = session;
            _session.StartTransaction();
        }

        /// <inheritdoc/>
        public IClientSessionHandle? Session => _session;

        /// <inheritdoc/>
#pragma warning disable IDISP007
        public void Dispose() => _session.Dispose();
#pragma warning restore IDISP007

        /// <inheritdoc/>
        public void Commit(CancellationToken cancellationToken = default) => _session.CommitTransaction(cancellationToken);
    }
}

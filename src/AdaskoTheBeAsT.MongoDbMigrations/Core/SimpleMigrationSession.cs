using System.Threading;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// Simple migration session without transaction support.
/// </summary>
public sealed class SimpleMigrationSession : IMigrationSession
{
    /// <inheritdoc/>
    public IMigrationTransaction BeginTransaction() => new SimpleTransaction();

    private sealed class SimpleTransaction : IMigrationTransaction
    {
        /// <inheritdoc/>
        public IClientSessionHandle? Session => null;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public void Commit(CancellationToken cancellationToken = default)
        {
        }
    }
}

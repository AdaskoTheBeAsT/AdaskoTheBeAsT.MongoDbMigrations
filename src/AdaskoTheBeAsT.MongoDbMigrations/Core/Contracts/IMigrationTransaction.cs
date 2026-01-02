using System;
using System.Threading;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

/// <summary>
/// Interface for migration transaction handling.
/// </summary>
public interface IMigrationTransaction
    : IDisposable
{
    /// <summary>
    /// Gets the client session handle, null for simple transactions.
    /// </summary>
    IClientSessionHandle? Session { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    void Commit(CancellationToken cancellationToken = default);
}

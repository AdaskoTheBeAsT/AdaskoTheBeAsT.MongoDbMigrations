using System;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

/// <summary>
/// Interface for migration runner configuration and execution.
/// </summary>
public interface IMigrationRunner
{
    /// <summary>
    /// Sets a progress handler that will be called after each migration.
    /// </summary>
    /// <param name="action">Action to handle progress.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseProgressHandler(Action<InterimMigrationResult> action);

    /// <summary>
    /// Sets a cancellation token for the migration process.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseCancellationToken(CancellationToken token);

    /// <summary>
    /// Sets a custom name for the specification collection.
    /// </summary>
    /// <param name="name">Collection name.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseCustomSpecificationCollectionName(string name);

    /// <summary>
    /// Enables dry run mode. When enabled, migrations are not executed but the result shows what would be executed.
    /// </summary>
    /// <param name="enabled">True to enable dry run mode.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseDryRun(bool enabled = true);

    /// <summary>
    /// Sets a hook that will be called before each migration is executed.
    /// </summary>
    /// <param name="action">Action to execute before migration. Receives the migration instance.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseBeforeMigration(Action<IMigration> action);

    /// <summary>
    /// Sets a hook that will be called after each migration is executed.
    /// </summary>
    /// <param name="action">Action to execute after migration. Receives the migration instance and success status.</param>
    /// <returns>The migration runner.</returns>
    IMigrationRunner UseAfterMigration(Action<IMigration, bool> action);

    /// <summary>
    /// Runs the migration to the specified version asynchronously.
    /// </summary>
    /// <param name="version">Target version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> RunAsync(Version version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the migration to the latest version asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> RunAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the specified number of migration steps.
    /// </summary>
    /// <param name="stepsBack">Number of migration steps to roll back.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Migration result.</returns>
    Task<MigrationResult> RollbackAsync(int stepsBack, CancellationToken cancellationToken = default);
}

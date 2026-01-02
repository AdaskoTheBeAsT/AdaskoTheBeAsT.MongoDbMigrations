using System.Threading.Tasks;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Interface for MongoDB migrations.
/// All migrations are async to match MongoDB driver patterns.
/// </summary>
public interface IMigration
{
    /// <summary>
    /// Gets the semantic version in format MAJOR.MINOR.REVISION.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Gets the name of migration.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Asynchronous roll forward method.
    /// </summary>
    /// <param name="context">Migration context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpAsync(MigrationContext context);

    /// <summary>
    /// Asynchronous roll back method.
    /// </summary>
    /// <param name="context">Migration context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DownAsync(MigrationContext context);
}

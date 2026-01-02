namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Specifies the direction of a migration operation.
/// </summary>
public enum MigrationDirection
{
    /// <summary>
    /// Used in Up migration method.
    /// </summary>
    Up,

    /// <summary>
    /// Used in Down migration method.
    /// </summary>
    Down,

    /// <summary>
    /// Used in both Up and Down migration methods.
    /// </summary>
    Both,
}

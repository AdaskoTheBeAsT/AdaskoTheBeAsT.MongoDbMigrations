using System;
using System.Diagnostics.CodeAnalysis;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Declares MongoDB collections used by this migration.
/// Applied by source generator or manually for schema validation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
public sealed class MigrationCollectionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationCollectionAttribute"/> class.
    /// </summary>
    /// <param name="collectionName">Name of the MongoDB collection.</param>
    /// <param name="direction">Migration direction in which this collection is used.</param>
    public MigrationCollectionAttribute(string collectionName, MigrationDirection direction = MigrationDirection.Both)
    {
        CollectionName = collectionName;
        Direction = direction;
    }

    /// <summary>
    /// Gets the name of the MongoDB collection.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Gets the migration direction in which this collection is used.
    /// </summary>
    public MigrationDirection Direction { get; }
}

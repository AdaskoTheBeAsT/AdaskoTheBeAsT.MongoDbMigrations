using System;
using System.Collections.Generic;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Describes a migration with its metadata.
/// Used by the generated migration registry.
/// </summary>
public sealed class MigrationDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationDescriptor"/> class.
    /// </summary>
    /// <param name="migrationType">The type of the migration class.</param>
    /// <param name="version">The migration version.</param>
    /// <param name="name">The migration name.</param>
    /// <param name="upCollections">Collection names used in Up method.</param>
    /// <param name="downCollections">Collection names used in Down method.</param>
    public MigrationDescriptor(
        Type migrationType,
        Version version,
        string name,
        IReadOnlyList<string> upCollections,
        IReadOnlyList<string> downCollections)
    {
        MigrationType = migrationType ?? throw new ArgumentNullException(nameof(migrationType));
        Version = version;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpCollections = upCollections ?? throw new ArgumentNullException(nameof(upCollections));
        DownCollections = downCollections ?? throw new ArgumentNullException(nameof(downCollections));
    }

    /// <summary>
    /// Gets the type of the migration class.
    /// </summary>
    public Type MigrationType { get; }

    /// <summary>
    /// Gets the migration version.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    /// Gets the migration name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collection names used in the Up method.
    /// </summary>
    public IReadOnlyList<string> UpCollections { get; }

    /// <summary>
    /// Gets the collection names used in the Down method.
    /// </summary>
    public IReadOnlyList<string> DownCollections { get; }

    /// <summary>
    /// Creates a new instance of the migration.
    /// </summary>
    /// <returns>A new migration instance.</returns>
    public IMigration CreateInstance()
    {
        return (IMigration)Activator.CreateInstance(MigrationType)!;
    }
}

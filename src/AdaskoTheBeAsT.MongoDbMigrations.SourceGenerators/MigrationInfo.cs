using System.Collections.Generic;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators;

/// <summary>
/// Holds information about a discovered migration.
/// </summary>
internal sealed class MigrationInfo
{
    public MigrationInfo(
        string fullTypeName,
        VersionInfo version,
        string name,
        IReadOnlyList<string> upCollections,
        IReadOnlyList<string> downCollections)
    {
        FullTypeName = fullTypeName;
        Version = version;
        Name = name;
        UpCollections = upCollections;
        DownCollections = downCollections;
    }

    public string FullTypeName { get; }

    public VersionInfo Version { get; }

    public string Name { get; }

    public IReadOnlyList<string> UpCollections { get; }

    public IReadOnlyList<string> DownCollections { get; }
}

using System.Collections.Generic;
using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

public class MigrationResult
{
    public Version CurrentVersion { get; set; }

    public IList<InterimMigrationResult> InterimSteps { get; set; } = new List<InterimMigrationResult>();

    public string? ServerAddress { get; set; }

    public string? DatabaseName { get; set; }

    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this was a dry run (no actual migrations executed).
    /// </summary>
    public bool IsDryRun { get; set; }
}

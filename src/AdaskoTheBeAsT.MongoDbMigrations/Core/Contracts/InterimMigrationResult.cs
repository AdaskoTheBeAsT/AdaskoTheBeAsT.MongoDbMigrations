using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

public class InterimMigrationResult
{
    public string? MigrationName { get; set; }

    public Version TargetVersion { get; set; }

    public string? ServerAddress { get; set; }

    public string? DatabaseName { get; set; }

    public int CurrentNumber { get; set; }

    public int TotalCount { get; set; }
}

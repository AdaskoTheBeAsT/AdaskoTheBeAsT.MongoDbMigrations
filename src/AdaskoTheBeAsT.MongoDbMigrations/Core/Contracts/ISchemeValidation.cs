namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

public interface ISchemeValidation
{
    IMigrationRunner UseSchemeValidation(bool enabled, string? migrationProjectLocation = null);
}

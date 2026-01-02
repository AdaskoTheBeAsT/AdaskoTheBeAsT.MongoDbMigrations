namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

public interface IMigrationSession
{
    IMigrationTransaction BeginTransaction();
}

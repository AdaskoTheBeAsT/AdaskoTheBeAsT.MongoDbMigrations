using MongoDB.Bson;
using MongoDB.Driver;
using IMigration = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IMigration;
using MigrationContext = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.MigrationContext;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Migrations;

public class FirstMigration : IMigration
{
    public Version Version => new Version(1, 0, 0);

    public string Name => "Rename name column to firstName";

    public Task UpAsync(MigrationContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>("clients");
        return collection.UpdateManyAsync(
            FilterDefinition<BsonDocument>.Empty,
            Builders<BsonDocument>.Update.Rename("name", "firstName"),
            cancellationToken: context.CancellationToken);
    }

    public Task DownAsync(MigrationContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>("clients");
        return collection.UpdateManyAsync(
            FilterDefinition<BsonDocument>.Empty,
            Builders<BsonDocument>.Update.Rename("firstName", "name"),
            cancellationToken: context.CancellationToken);
    }
}

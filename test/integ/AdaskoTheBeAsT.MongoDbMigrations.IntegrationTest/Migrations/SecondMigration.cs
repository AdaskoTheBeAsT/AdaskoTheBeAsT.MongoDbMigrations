using MongoDB.Bson;
using MongoDB.Driver;
using IMigration = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IMigration;
using MigrationContext = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.MigrationContext;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Migrations;

public class SecondMigration : IMigration
{
    public Version Version => new Version(1, 1, 0);

    public string Name => "Convert age to string";

    public async Task UpAsync(MigrationContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>("clients");
        using var cursor = await collection.FindAsync(
            FilterDefinition<BsonDocument>.Empty,
            cancellationToken: context.CancellationToken).ConfigureAwait(false);
        var list = await cursor.ToListAsync(context.CancellationToken).ConfigureAwait(false);
        FieldDefinition<BsonDocument, string?> fieldDefinition = "age";
        foreach (var item in list)
        {
            await collection.UpdateOneAsync(
                new BsonDocument("_id", item["_id"]),
                Builders<BsonDocument>.Update.Set(fieldDefinition, item["age"].ToString()),
                cancellationToken: context.CancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DownAsync(MigrationContext context)
    {
        var collection = context.Database.GetCollection<BsonDocument>("clients");
        using var cursor = await collection.FindAsync(
            FilterDefinition<BsonDocument>.Empty,
            cancellationToken: context.CancellationToken).ConfigureAwait(false);
        var list = await cursor.ToListAsync(context.CancellationToken).ConfigureAwait(false);
        FieldDefinition<BsonDocument, int> fieldDefinition = "age";
        foreach (var item in list)
        {
            await collection.UpdateOneAsync(
                new BsonDocument("_id", item["_id"]),
                Builders<BsonDocument>.Update.Set(fieldDefinition, item["age"].ToInt32()),
                cancellationToken: context.CancellationToken).ConfigureAwait(false);
        }
    }
}

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Document;

public class SpecificationItem
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string Id { get; set; } = string.Empty;

    [BsonElement("n")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("v")]
    public Version Ver { get; set; }

    [BsonElement("d")]
    public bool IsUp { get; set; }

    [BsonElement("applied")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc, Representation = BsonType.DateTime)]
    public DateTime ApplyingDateTime { get; set; }
}

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Document;

/// <summary>
/// BSON serializer for the Version struct.
/// </summary>
public class VersionSerializer : SerializerBase<Version>
{
    /// <inheritdoc/>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Version value)
    {
        context.Writer.WriteString(value.ToString());
    }

    /// <inheritdoc/>
    public override Version Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var ver = context.Reader.ReadString();
        return new Version(ver);
    }
}

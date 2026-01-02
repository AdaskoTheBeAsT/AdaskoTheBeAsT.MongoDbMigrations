using AdaskoTheBeAsT.MongoDbMigrations.Document;
using AwesomeAssertions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Test.Document;

public class VersionSerializerTests
{
    private readonly VersionSerializer _sut;

    public VersionSerializerTests()
    {
        _sut = new VersionSerializer();
    }

    [Fact]
    public void Serialize_ShouldWriteVersionAsString()
    {
        var version = new Version(1, 2, 3);
        using var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        var context = BsonSerializationContext.CreateRoot(writer);

        writer.WriteStartDocument();
        writer.WriteName(nameof(version));
        _sut.Serialize(context, default, version);
        writer.WriteEndDocument();
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var result = reader.ReadString();

        result.Should().Be("1.2.3");
    }

    [Fact]
    public void Deserialize_ShouldReadVersionFromString()
    {
        using var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        writer.WriteStartDocument();
        writer.WriteName("version");
        writer.WriteString("2.5.3");
        writer.WriteEndDocument();
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);

        var result = _sut.Deserialize(context, default);

        result.Major.Should().Be(2);
        result.Minor.Should().Be(5);
        result.Revision.Should().Be(3);
    }

    [Theory]
    [InlineData("0.0.0", 0, 0, 0)]
    [InlineData("1.0.0", 1, 0, 0)]
    [InlineData("99.99.99", 99, 99, 99)]
    public void RoundTrip_ShouldPreserveVersion(
        string versionString,
        int major,
        int minor,
        int revision)
    {
        var originalVersion = new Version(versionString);

        using var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        var serializeContext = BsonSerializationContext.CreateRoot(writer);

        writer.WriteStartDocument();
        writer.WriteName("version");
        _sut.Serialize(serializeContext, default, originalVersion);
        writer.WriteEndDocument();
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var deserializeContext = BsonDeserializationContext.CreateRoot(reader);

        var result = _sut.Deserialize(deserializeContext, default);

        result.Major.Should().Be(major);
        result.Minor.Should().Be(minor);
        result.Revision.Should().Be(revision);
    }
}

using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Util;
using AwesomeAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Reqnroll;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Steps;

[Binding]
public class SchemaValidationSteps(ScenarioContext scenarioContext)
{
    private readonly string _connectionString = scenarioContext.Get<string>("ConnectionString");
#pragma warning disable CC0032 // Dispose Fields Properly
    private readonly IMongoClient _client = scenarioContext.Get<IMongoClient>();
#pragma warning restore CC0032 // Dispose Fields Properly

    private string DatabaseName => scenarioContext.Get<string>(nameof(DatabaseName));

    [Given(@"the collection has documents with inconsistent schema")]
    public async Task GivenTheCollectionHasDocumentsWithInconsistentSchemaAsync(DataTable table)
    {
        var db = DbHelper.GetDatabase(_client, DatabaseName);
        var collection = db.GetCollection<BsonDocument>("clients");

        foreach (var row in table.Rows)
        {
            var doc = new BsonDocument { { "name", row["name"] } };

            if (!string.IsNullOrEmpty(row["isActive"]))
            {
                doc.Add("isActive", bool.Parse(row["isActive"]));
            }

            await collection.InsertOneAsync(doc);
        }
    }

    [When(@"I run migration to version ""(.*)"" with schema validation")]
    public async Task WhenIRunMigrationToVersionWithSchemaValidationAsync(string version)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(enabled: true, GetProjectPath())
            .RunAsync(new Version(version));

        scenarioContext.Set(result, "MigrationResult");
    }

    [When(@"I run migration to version ""(.*)"" with schema validation expecting failure")]
    public async Task WhenIRunMigrationToVersionWithSchemaValidationExpectingFailureAsync(string version)
    {
        Exception? caughtException = null;

        try
        {
            using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
            var result = await engine
                .UseAssembly(Assembly.GetExecutingAssembly())
                .UseSchemeValidation(enabled: true, GetProjectPath())
                .RunAsync(new Version(version));

            scenarioContext.Set(result, "MigrationResult");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        if (caughtException != null)
        {
            scenarioContext.Set(caughtException, "CaughtException");
        }
    }

    [Then(@"the migration should throw an exception")]
    public void ThenTheMigrationShouldThrowAnException()
    {
        var exception = scenarioContext.Get<Exception>("CaughtException");
        exception.Should().NotBeNull();
    }

    private static string GetProjectPath()
    {
        var finder = new DirectoryInfo(Directory.GetCurrentDirectory());
        FileInfo? file = null;

        while (finder != null)
        {
            file = finder.EnumerateFiles("AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.csproj").FirstOrDefault();
            if (file != null)
            {
                break;
            }

            finder = finder.Parent;
        }

        return file?.FullName ?? throw new InvalidOperationException(
            $"Could not find AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.csproj starting from {Directory.GetCurrentDirectory()}");
    }
}

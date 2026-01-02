using System.Globalization;
using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Util;
using AwesomeAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Reqnroll;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Steps;

[Binding]
public class MigrationSteps(ScenarioContext scenarioContext)
{
    private readonly string _connectionString = scenarioContext.Get<string>("ConnectionString");
#pragma warning disable CC0032 // Dispose Fields Properly
    private readonly IMongoClient _client = scenarioContext.Get<IMongoClient>();
#pragma warning restore CC0032 // Dispose Fields Properly

    private string DatabaseName
    {
        get => scenarioContext.Get<string>(nameof(DatabaseName));
        set => scenarioContext.Set(value, nameof(DatabaseName));
    }

    [BeforeScenario(Order = 10)]
    public void BeforeScenario()
    {
        DatabaseName = DbHelper.CreateTestDatabase();
    }

    [AfterScenario(Order = 10)]
    public async Task AfterScenarioAsync()
    {
        if (scenarioContext.TryGetValue<string>(nameof(DatabaseName), out var dbName) && !string.IsNullOrEmpty(dbName))
        {
            await DbHelper.DropDatabaseAsync(_client, dbName);
        }
    }

    [Given(@"a MongoDB database with clients collection")]
    public Task GivenAMongoDbDatabaseWithClientsCollectionAsync()
    {
        var db = DbHelper.GetDatabase(_client, DatabaseName);
        return db.CreateCollectionAsync("clients");
    }

    [Given(@"the collection has the following documents")]
    public async Task GivenTheCollectionHasTheFollowingDocumentsAsync(DataTable table)
    {
        var db = DbHelper.GetDatabase(_client, DatabaseName);
        var collection = db.GetCollection<BsonDocument>("clients");

        foreach (var row in table.Rows)
        {
            var doc = new BsonDocument
            {
                { "name", row["name"] },
                { "age", int.Parse(row["age"], CultureInfo.InvariantCulture) },
            };
            await collection.InsertOneAsync(doc);
        }
    }

    [Given(@"the database is at version ""(.*)""")]
    public async Task GivenTheDatabaseIsAtVersionAsync(string version)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .RunAsync(new Version(version));
    }

    [When(@"I run migration to version ""(.*)""")]
    public async Task WhenIRunMigrationToVersionAsync(string version)
    {
        Exception? caughtException = null;

        try
        {
            using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
            var result = await engine
                .UseAssembly(Assembly.GetExecutingAssembly())
                .UseSchemeValidation(false)
                .RunAsync(new Version(version));

            scenarioContext.Set(result, nameof(MigrationResult));
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

    [When(@"I run migration to latest version")]
    public async Task WhenIRunMigrationToLatestVersionAsync()
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .RunAsync();

        scenarioContext.Set(result, nameof(MigrationResult));
    }

    [When(@"I rollback (.*) migration step")]
    public async Task WhenIRollbackMigrationStepsAsync(int steps)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .RollbackAsync(steps);

        scenarioContext.Set(result, nameof(MigrationResult));
    }

    [When(@"I run dry run migration to version ""(.*)""")]
    public async Task WhenIRunDryRunMigrationToVersionAsync(string version)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .UseDryRun(true)
            .RunAsync(new Version(version));

        scenarioContext.Set(result, nameof(MigrationResult));
    }

    [Then(@"the migration should succeed")]
    public void ThenTheMigrationShouldSucceed()
    {
        var result = scenarioContext.Get<MigrationResult>(nameof(MigrationResult));
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Then(@"the database version should be ""(.*)""")]
    public void ThenTheDatabaseVersionShouldBe(string version)
    {
        var result = scenarioContext.Get<MigrationResult>(nameof(MigrationResult));
        result.Should().NotBeNull();
        result.CurrentVersion.Should().Be(new Version(version));
    }

    [Then(@"the result should indicate dry run")]
    public void ThenTheResultShouldIndicateDryRun()
    {
        var result = scenarioContext.Get<MigrationResult>(nameof(MigrationResult));
        result.Should().NotBeNull();
        result.IsDryRun.Should().BeTrue();
    }

    [Then(@"the documents should still have ""(.*)"" field")]
    public async Task ThenTheDocumentsShouldStillHaveFieldAsync(string fieldName)
    {
        var db = DbHelper.GetDatabase(_client, DatabaseName);
        var doc = await db.GetCollection<BsonDocument>("clients")
            .Find(FilterDefinition<BsonDocument>.Empty)
            .FirstOrDefaultAsync();

        doc.Should().NotBeNull();
        doc!.Contains(fieldName).Should().BeTrue();
    }
}

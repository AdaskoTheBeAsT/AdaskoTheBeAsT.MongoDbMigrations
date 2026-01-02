using MongoDB.Driver;
using Reqnroll;
using Testcontainers.MongoDb;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Hooks;

[Binding]
public sealed class GlobalHooks
{
    private static MongoDbContainer? _container;
    private static IMongoClient? _client;

    [BeforeTestRun]
    public static async Task StartMongoAsync()
    {
        _container = new MongoDbBuilder("mongo:8.2.3").Build();
        await _container.StartAsync();
        _client = new MongoClient(_container.GetConnectionString());
    }

    [AfterTestRun]
    public static async Task StopMongoAsync()
    {
        _client?.Dispose();
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [BeforeScenario(Order = 1)]
    public void Register(ScenarioContext ctx)
    {
        ctx.Set(_container!.GetConnectionString(), "ConnectionString");
        ctx.Set(_client!);
    }
}

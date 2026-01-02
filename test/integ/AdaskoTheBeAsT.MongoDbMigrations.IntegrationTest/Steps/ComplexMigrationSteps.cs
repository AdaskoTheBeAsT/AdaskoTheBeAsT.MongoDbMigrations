using System.Reflection;
using AwesomeAssertions;
using Reqnroll;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Steps;

[Binding]
public class ComplexMigrationSteps(ScenarioContext scenarioContext)
{
    private readonly string _connectionString = scenarioContext.Get<string>("ConnectionString");

    private string DatabaseName => scenarioContext.Get<string>(nameof(DatabaseName));

    [When(@"I run migration down to version ""(.*)""")]
    public async Task WhenIRunMigrationDownToVersionAsync(string version)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .RunAsync(new Version(version));

        scenarioContext.Set(result, "MigrationResult");
    }

    [When(@"I run migration to version ""(.*)"" with hooks")]
    public async Task WhenIRunMigrationToVersionWithHooksAsync(string version)
    {
        var beforeCount = 0;
        var afterCount = 0;

        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .UseBeforeMigration(_ => beforeCount++)
            .UseAfterMigration((_, _) => afterCount++)
            .RunAsync(new Version(version));

        scenarioContext.Set(result, "MigrationResult");
        scenarioContext.Set(beforeCount, "BeforeHookCount");
        scenarioContext.Set(afterCount, "AfterHookCount");
    }

    [Then(@"the before hook should be called (.*) time")]
    public void ThenTheBeforeHookShouldBeCalledTimes(int expectedCount)
    {
        var count = scenarioContext.Get<int>("BeforeHookCount");
        count.Should().Be(expectedCount);
    }

    [Then(@"the after hook should be called (.*) time")]
    public void ThenTheAfterHookShouldBeCalledTimes(int expectedCount)
    {
        var count = scenarioContext.Get<int>("AfterHookCount");
        count.Should().Be(expectedCount);
    }

    [When(@"I rollback (.*) migration steps")]
    public async Task WhenIRollbackMultipleMigrationStepsAsync(int steps)
    {
        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .RollbackAsync(steps);

        scenarioContext.Set(result, "MigrationResult");
    }

    [When(@"I run migration to version ""(.*)"" with cancellation")]
    public async Task WhenIRunMigrationToVersionWithCancellationAsync(string version)
    {
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
        await Task.CompletedTask;
#endif

        Exception? caughtException = null;

        try
        {
            using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
            await engine
                .UseAssembly(Assembly.GetExecutingAssembly())
                .UseSchemeValidation(false)
                .RunAsync(new Version(version), cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            caughtException = ex;
        }

        if (caughtException != null)
        {
            scenarioContext.Set(caughtException, "CaughtException");
        }
    }

    [Then(@"the migration should be cancelled")]
    public void ThenTheMigrationShouldBeCancelled()
    {
        var exception = scenarioContext.Get<Exception>("CaughtException");
        exception.Should().BeOfType<OperationCanceledException>();
    }
}

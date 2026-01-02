using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AdaskoTheBeAsT.MongoDbMigrations.Exceptions;
using AwesomeAssertions;
using Reqnroll;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Steps;

[Binding]
public class SimpleUpDownSteps(ScenarioContext scenarioContext)
{
    private readonly string _connectionString = scenarioContext.Get<string>("ConnectionString");

    private string DatabaseName => scenarioContext.Get<string>(nameof(DatabaseName));

    [When(@"I run migration to version ""(.*)"" with progress handler")]
    public async Task WhenIRunMigrationToVersionWithProgressHandlerAsync(string version)
    {
        var actions = new List<string>();

        using var engine = new MigrationEngineBuilder().UseDatabase(_connectionString, DatabaseName);
        var result = await engine
            .UseAssembly(Assembly.GetExecutingAssembly())
            .UseSchemeValidation(false)
            .UseProgressHandler(i => actions.Add(i.MigrationName ?? string.Empty))
            .RunAsync(new Version(version));

        scenarioContext.Set(result, nameof(MigrationResult));
        scenarioContext.Set(actions, "ProgressActions");
    }

    [Then(@"the interim steps count should be greater than 0")]
    public void ThenTheInterimStepsCountShouldBeGreaterThan0()
    {
        var result = scenarioContext.Get<MigrationResult>(nameof(MigrationResult));
        result.InterimSteps.Count.Should().BeGreaterThan(0);
    }

    [Then(@"the progress handler should be called for each step")]
    public void ThenTheProgressHandlerShouldBeCalledForEachStep()
    {
        var result = scenarioContext.Get<MigrationResult>(nameof(MigrationResult));
        var actions = scenarioContext.Get<List<string>>("ProgressActions");

        actions.Count.Should().Be(result.InterimSteps.Count);
    }

    [Then(@"a MigrationNotFoundException should be thrown")]
    public void ThenAMigrationNotFoundExceptionShouldBeThrown()
    {
        var exception = scenarioContext.Get<Exception>("CaughtException");
        exception.Should().BeOfType<MigrationNotFoundException>();
    }
}

using AdaskoTheBeAsT.MongoDbMigrations.Exceptions;
using AwesomeAssertions;
using Reqnroll;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Steps;

[Binding]
public class DatabaseCheckerSteps(ScenarioContext scenarioContext)
{
    private readonly string _connectionString = scenarioContext.Get<string>("ConnectionString");

    private string DatabaseName => scenarioContext.Get<string>(nameof(DatabaseName));

    [When(@"I check if the database is outdated")]
    public void WhenICheckIfTheDatabaseIsOutdated()
    {
        var result = MongoDatabaseStateChecker.IsDatabaseOutdated(
            _connectionString,
            DatabaseName,
            typeof(DatabaseCheckerSteps).Assembly);

        scenarioContext.Set(result, "IsDatabaseOutdated");
    }

    [When(@"I call ThrowIfDatabaseOutdated")]
    public void WhenICallThrowIfDatabaseOutdated()
    {
        Exception? caughtException = null;

        try
        {
            MongoDatabaseStateChecker.ThrowIfDatabaseOutdated(
                _connectionString,
                DatabaseName,
                typeof(DatabaseCheckerSteps).Assembly);
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

    [Then(@"the result should indicate database is outdated")]
    public void ThenTheResultShouldIndicateDatabaseIsOutdated()
    {
        var result = scenarioContext.Get<bool>("IsDatabaseOutdated");
        result.Should().BeTrue();
    }

    [Then(@"the result should indicate database is not outdated")]
    public void ThenTheResultShouldIndicateDatabaseIsNotOutdated()
    {
        var result = scenarioContext.Get<bool>("IsDatabaseOutdated");
        result.Should().BeFalse();
    }

    [Then(@"a DatabaseOutdatedException should be thrown")]
    public void ThenADatabaseOutdatedExceptionShouldBeThrown()
    {
        var exception = scenarioContext.Get<Exception>("CaughtException");
        exception.Should().BeOfType<DatabaseOutdatedException>();
    }
}

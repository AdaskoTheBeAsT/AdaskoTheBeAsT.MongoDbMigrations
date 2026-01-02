using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AdaskoTheBeAsT.MongoDbMigrations.Document;
using AdaskoTheBeAsT.MongoDbMigrations.Exceptions;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations;

/// <summary>
/// Helper class to check if database is outdated.
/// </summary>
public static class MongoDatabaseStateChecker
{
    /// <summary>
    /// Throws an exception if the database is outdated.
    /// </summary>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="databaseName">Database name.</param>
    /// <param name="migrationAssembly">Assembly containing migrations.</param>
    /// <param name="emulation">MongoDB emulation type.</param>
    /// <exception cref="DatabaseOutdatedException">Thrown when database is outdated.</exception>
    public static void ThrowIfDatabaseOutdated(
        string connectionString,
        string databaseName,
        Assembly? migrationAssembly = null,
        MongoEmulation emulation = MongoEmulation.None)
    {
        var (dbVersion, availableVersion) = GetCurrentVersions(
            connectionString,
            databaseName,
            migrationAssembly,
            emulation);
        if (availableVersion > dbVersion)
        {
            throw new DatabaseOutdatedException(dbVersion, availableVersion);
        }
    }

    /// <summary>
    /// Checks if the database is outdated.
    /// </summary>
    /// <param name="connectionString">MongoDB connection string.</param>
    /// <param name="databaseName">Database name.</param>
    /// <param name="migrationAssembly">Assembly containing migrations.</param>
    /// <param name="emulation">MongoDB emulation type.</param>
    /// <returns>True if database is outdated, false otherwise.</returns>
    public static bool IsDatabaseOutdated(
        string connectionString,
        string databaseName,
        Assembly? migrationAssembly = null,
        MongoEmulation emulation = MongoEmulation.None)
    {
        var (dbVersion, availableVersion) = GetCurrentVersions(
            connectionString,
            databaseName,
            migrationAssembly,
            emulation);
        return availableVersion > dbVersion;
    }

    private static (Version DbVersion, Version AvailableVersion) GetCurrentVersions(
        string connectionString,
        string databaseName,
        Assembly? migrationAssembly,
        MongoEmulation emulation)
    {
        var locator = new MigrationManager();
        if (migrationAssembly != null)
        {
            locator.SetAssembly(migrationAssembly);
        }

        var highestAvailableVersion = locator.GetNewestLocalVersion();

        using var client = new MongoClient(connectionString);
        var dbStatus = new DatabaseManager(client.GetDatabase(databaseName), emulation);
        var currentDbVersion = dbStatus.GetVersion();

        return (currentDbVersion, highestAvailableVersion);
    }
}

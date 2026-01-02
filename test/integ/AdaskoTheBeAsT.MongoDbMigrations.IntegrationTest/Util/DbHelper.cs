using System.Runtime.InteropServices;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.IntegrationTest.Util;

public static class DbHelper
{
    private const string DatabaseName = "migrationtest";

    /// <summary>
    /// Gets a short runtime identifier to distinguish between different .NET versions.
    /// Must be short to fit MongoDB's 63 character database name limit.
    /// </summary>
    private static string RuntimeId
    {
        get
        {
            var desc = RuntimeInformation.FrameworkDescription;

            // Extract short version identifier
            if (desc.Contains("NET 10"))
            {
                return "n10";
            }

            if (desc.Contains("NET 9"))
            {
                return "n9";
            }

            if (desc.Contains("NET 8"))
            {
                return "n8";
            }

            if (desc.Contains(".NET Framework 4.8.1"))
            {
                return "f481";
            }

            if (desc.Contains(".NET Framework 4.8"))
            {
                return "f48";
            }

            if (desc.Contains(".NET Framework 4.7.2"))
            {
                return "f472";
            }

            // Fallback: use hash of description
            return StringComparer.Ordinal.GetHashCode(desc).ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Creates a fresh database for a test.
    /// </summary>
    /// <returns>A unique database name for this test including runtime identifier.</returns>
    public static string CreateTestDatabase()
    {
        // Format: migrationtest_n9_<guid> = ~50 chars (well under 63 limit)
        return $"{DatabaseName}_{RuntimeId}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Gets a MongoDB database.
    /// </summary>
    /// <param name="client">The MongoDB client.</param>
    /// <param name="databaseName">The database name.</param>
    public static IMongoDatabase GetDatabase(IMongoClient client, string databaseName)
    {
        return client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Drops a database after test.
    /// </summary>
    /// <param name="client">The MongoDB client.</param>
    /// <param name="databaseName">Database to drop.</param>
    public static Task DropDatabaseAsync(IMongoClient client, string databaseName)
    {
        return client.DropDatabaseAsync(databaseName);
    }
}

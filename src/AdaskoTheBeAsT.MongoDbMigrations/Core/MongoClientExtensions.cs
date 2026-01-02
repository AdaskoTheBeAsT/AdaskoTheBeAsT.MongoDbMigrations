using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// Extension methods for IMongoClient.
/// </summary>
internal static class MongoClientExtensions
{
    /// <summary>
    /// Configures TLS settings for the MongoDB client.
    /// </summary>
    /// <param name="instance">The MongoDB client instance.</param>
    /// <param name="config">SSL settings configuration.</param>
    /// <returns>The configured MongoDB client.</returns>
    public static IMongoClient SetTls(this IMongoClient instance, SslSettings? config)
    {
        if (config != null)
        {
            instance.Settings.UseTls = true;
            instance.Settings.SslSettings = config;
        }

        return instance;
    }

    /// <summary>
    /// Configures SSH tunnel settings for the MongoDB client.
    /// </summary>
    /// <param name="instance">The MongoDB client instance.</param>
    /// <param name="config">SSH configuration.</param>
    /// <returns>The configured MongoDB client.</returns>
    public static IMongoClient SetSsh(this IMongoClient instance, SshConfig? config)
    {
        if (config != null)
        {
            instance.Settings.Server = new MongoServerAddress(config.BoundHost, checked((int)config.BoundPort));
        }

        return instance;
    }
}

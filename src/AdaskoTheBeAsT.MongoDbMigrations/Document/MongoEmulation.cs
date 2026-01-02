namespace AdaskoTheBeAsT.MongoDbMigrations.Document;

/// <summary>
/// MongoDB emulation types.
/// </summary>
public enum MongoEmulation
{
    /// <summary>
    /// No emulation.
    /// </summary>
    None,

    /// <summary>
    /// Azure Cosmos DB emulation.
    /// </summary>
    AzureCosmos,

    /// <summary>
    /// AWS DocumentDB emulation.
    /// </summary>
    AwsDocument,
}

using System;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Exceptions;

/// <summary>
/// Exception thrown when the database version is outdated.
/// </summary>
public class DatabaseOutdatedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOutdatedException"/> class.
    /// </summary>
    public DatabaseOutdatedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOutdatedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DatabaseOutdatedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOutdatedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DatabaseOutdatedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOutdatedException"/> class.
    /// </summary>
    /// <param name="databaseVersion">Current database version.</param>
    /// <param name="targetVersion">Target version required.</param>
    public DatabaseOutdatedException(Version databaseVersion, Version targetVersion)
        : base($"Current database version: {databaseVersion}. You must update database to {targetVersion}.")
    {
    }
}

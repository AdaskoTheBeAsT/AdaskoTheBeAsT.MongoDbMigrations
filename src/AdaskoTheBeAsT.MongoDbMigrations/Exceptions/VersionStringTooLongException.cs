using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Exceptions;

public class VersionStringTooLongException : Exception
{
    public VersionStringTooLongException()
    {
    }

    public VersionStringTooLongException(string version)
        : base($"Versions must have the format: major.minor.revision, this doesn't match: {version}")
    {
    }

    public VersionStringTooLongException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Exceptions;

public class InvalidVersionException : Exception
{
    public InvalidVersionException()
    {
    }

    public InvalidVersionException(string version)
        : base($"Invalid value: {version}")
    {
    }

    public InvalidVersionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

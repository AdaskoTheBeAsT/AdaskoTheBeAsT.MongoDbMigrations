using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Exceptions;

public class MigrationNotFoundException
    : Exception
{
    public MigrationNotFoundException(
        string assemblyName,
        Exception? innerException)
        : base($"Migrations are not found in assembly {assemblyName}", innerException)
    {
    }

    public MigrationNotFoundException()
    {
    }

    public MigrationNotFoundException(string message)
        : base(message)
    {
    }
}

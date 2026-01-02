using System;
using System.Reflection;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

/// <summary>
/// Interface for locating migrations in assemblies.
/// </summary>
public interface ILocator
{
    ISchemeValidation UseAssemblyOfType(Type type);

    ISchemeValidation UseAssemblyOfType<T>();

    ISchemeValidation UseAssembly(Assembly assembly);
}

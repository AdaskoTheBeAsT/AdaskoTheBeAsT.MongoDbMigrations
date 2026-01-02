using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Marks a migration class to be ignored during migration discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreMigrationAttribute : Attribute
{
}

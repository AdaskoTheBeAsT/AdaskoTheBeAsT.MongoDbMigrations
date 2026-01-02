using System;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Marks a class as a generated migration registry.
/// This attribute is applied by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedMigrationRegistryAttribute : Attribute;

using Microsoft.CodeAnalysis;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators;

/// <summary>
/// Diagnostic descriptors for the migration source generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MigrationVersionNotConstant = new(
        id: "MONGO001",
        title: "Migration version must be a constant",
        messageFormat: "Migration '{0}' version property must return a constant value",
        category: "AdaskoTheBeAsT.MongoDbMigrations",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The Version property of a migration must return a constant value that can be evaluated at compile time.");

    public static readonly DiagnosticDescriptor DuplicateMigrationVersion = new(
        id: "MONGO002",
        title: "Duplicate migration version",
        messageFormat: "Migration version {0} is used by multiple migrations: {1}",
        category: "AdaskoTheBeAsT.MongoDbMigrations",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each migration must have a unique version number.");

    public static readonly DiagnosticDescriptor DynamicCollectionName = new(
        id: "MONGO003",
        title: "Dynamic collection name detected",
        messageFormat: "Collection name in '{0}' method could not be determined at compile time. Consider using [MigrationCollection] attribute.",
        category: "AdaskoTheBeAsT.MongoDbMigrations",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Collection names that cannot be determined at compile time should be declared using the MigrationCollectionAttribute.");

    public static readonly DiagnosticDescriptor MigrationMissingVersion = new(
        id: "MONGO004",
        title: "Migration missing Version property",
        messageFormat: "Migration '{0}' does not have a valid Version property with a constant value",
        category: "AdaskoTheBeAsT.MongoDbMigrations",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each migration must have a Version property that returns a constant value.");
}

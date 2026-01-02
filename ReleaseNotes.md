# AdaskoTheBeAsT.MongoDbMigrations Release Notes

## v3.0.0

### Breaking Changes

- **Package Split**: Library split into three packages for optimal deployment:
  - `AdaskoTheBeAsT.MongoDbMigrations.Abstractions` - Interfaces and types (`IMigration`, `Version`, `MigrationContext`)
  - `AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators` - Compile-time migration registry generator
  - `AdaskoTheBeAsT.MongoDbMigrations` - Runtime migration engine

- **Source Generator Architecture**: Replaced runtime Roslyn analysis with compile-time source generators
  - Eliminates ~8MB Microsoft.CodeAnalysis runtime dependency
  - Migration metadata extracted at build time
  - Duplicate version detection at compile time
  - Native AOT compatible

- **Async API**: All synchronous methods replaced with async equivalents
  - `Run()` → `RunAsync()`
  - `Up(IMongoDatabase)` → `UpAsync(MigrationContext)`
  - `Down(IMongoDatabase)` → `DownAsync(MigrationContext)`

- **MigrationContext**: New context object passed to migration methods providing:
  - `Database` - `IMongoDatabase` instance
  - `Session` - `IClientSessionHandle` for transaction support
  - `CancellationToken` - For cancellation support

- **MigrationEngineBuilder**: `MigrationEngine` replaced with builder pattern
  - Now implements `IDisposable`
  - Fluent API for configuration

- **Namespace Changes**: 
  - `MongoDBMigrations` → `AdaskoTheBeAsT.MongoDbMigrations`
  - `Version` class moved to `AdaskoTheBeAsT.MongoDbMigrations.Abstractions`

### New Features

- **Compile-time Migration Registry**: Auto-generated `MigrationRegistry` class with all migration metadata
- **Collection Name Extraction**: Source generator extracts collection names from `UpAsync`/`DownAsync` methods
- **Dry Run Mode**: Test migrations without applying changes via `UseDryRun(true)`
- **Rollback by Steps**: `RollbackAsync(steps)` to rollback multiple migrations
- **Migration Hooks**: `UseBeforeMigration()` and `UseAfterMigration()` callbacks
- **Full Transaction Support**: Via `MigrationContext.Session`
- **Cancellation Support**: Built-in `CancellationToken` in `MigrationContext`

### Improvements

- Faster startup (no runtime source parsing)
- Smaller deployment size (no CodeAnalysis assemblies)
- Better error messages with compile-time diagnostics
- Full async/await support matching MongoDB driver patterns
- Support for .NET Framework 4.7.2, 4.8, 4.8.1 and .NET 8.0, 9.0, 10.0

### Acknowledgments

This library is based on the excellent work of:
- [Artur Osmokiesku](https://bitbucket.org/i_am_a_kernel/mongodbmigrations/) - Original MongoDBMigrations author
- [Ruxo Zheng](https://github.com/ruxo/MongoDbMigrations) - MongoDBMigrationsRZ fork maintainer

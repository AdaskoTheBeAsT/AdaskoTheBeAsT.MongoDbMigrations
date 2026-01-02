using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
using AdaskoTheBeAsT.MongoDbMigrations.Exceptions;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// Works with local migrations.
/// </summary>
internal class MigrationManager
{
    private Assembly? _assembly;

    //// private IReadOnlyList<MigrationDescriptor>? _generatedRegistry;

    /// <summary>
    /// Migration manager constructor.
    /// </summary>
    public MigrationManager()
    {
    }

    /// <summary>
    /// Find all migrations in specific assembly.
    /// </summary>
    /// <param name="assembly">Assembly with migrations classes.</param>
    /// <returns>List of all found migrations.</returns>
    public static List<IMigration> GetAllMigrations(Assembly assembly)
    {
        var registry = FindGeneratedRegistry(assembly);
        if (registry != null && registry.Count > 0)
        {
            return registry.Select(d => d.CreateInstance()).ToList();
        }

        return GetAllMigrationsViaReflection(assembly);
    }

    /// <summary>
    /// Sets assembly.
    /// </summary>
    /// <param name="assembly">Assembly where migration classes located.</param>
    public void SetAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        _assembly = assembly;
    }

    /// <summary>
    /// Set assembly for finding migrations.
    /// </summary>
    /// <typeparam name="T">Type in assembly with migrations.</typeparam>
    public void LookInAssemblyOfType<T>()
    {
        var assembly = typeof(T).Assembly;
        _assembly = assembly;
    }

    /// <summary>
    /// Set assembly for finding migrations.
    /// </summary>
    /// <param name="type">Type in assembly with migrations.</param>
    public void LookInAssemblyOfType(Type type)
    {
        var assembly = type.Assembly;
        _assembly = assembly;
    }

    /// <summary>
    /// Find all migrations in executing assembly or assembly which found by <see cref="LookInAssemblyOfType"/> method.
    /// </summary>
    /// <returns>List of all found migrations.</returns>
    public List<IMigration> GetAllMigrations()
    {
        if (_assembly != null)
        {
            return GetAllMigrations(_assembly);
        }

        // Ok no problem let's try to find migrations in executing assembly
        var stackFrames = new StackTrace().GetFrames();
        if (stackFrames == null)
        {
            throw new InvalidOperationException("Can't find assembly with migrations. Try use LookInAssemblyOfType() method before.");
        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var callingFrame = stackFrames
            .FirstOrDefault(a => a.GetMethod()?.DeclaringType?.Assembly != currentAssembly);

        var trueCallingAssembly = callingFrame?.GetMethod()?.DeclaringType?.Assembly;

        if (trueCallingAssembly == null)
        {
            throw new InvalidOperationException("Can't find assembly with migrations. Try use LookInAssemblyOfType() method before.");
        }

        return GetAllMigrations(trueCallingAssembly);
    }

    /// <summary>
    /// Find all migrations in executing assembly or assembly which found by <see cref="LookInAssemblyOfType"/> method.
    /// Between current and target versions.
    /// </summary>
    /// <param name="currentVersion">Version of database.</param>
    /// <param name="targetVersion">Target version for migrating.</param>
    /// <returns>List of all found migrations.</returns>
    public List<IMigration> GetMigrations(Version currentVersion, Version targetVersion)
    {
        var migrations = GetAllMigrations();
        if (migrations.All(x => x.Version != targetVersion) && targetVersion != Version.Zero())
        {
            throw new MigrationNotFoundException(_assembly?.FullName ?? string.Empty, null);
        }

        if (targetVersion > currentVersion)
        {
            migrations = migrations
                .Where(x => x.Version > currentVersion && x.Version <= targetVersion)
                .OrderBy(x => x.Version).ToList();
        }
        else if (targetVersion < currentVersion)
        {
            migrations = migrations
                .Where(x => x.Version <= currentVersion && x.Version > targetVersion)
                .OrderByDescending(x => x.Version).ToList();
        }
        else
        {
            return Enumerable.Empty<IMigration>().ToList();
        }

        if (!migrations.Any())
        {
            throw new MigrationNotFoundException(_assembly?.FullName ?? string.Empty, null);
        }

        return migrations;
    }

    /// <summary>
    /// Gets collection names for the given migrations based on direction.
    /// Uses the generated registry if available.
    /// </summary>
    /// <param name="migrations">List of migrations.</param>
    /// <param name="isUp">True for Up direction, false for Down.</param>
    /// <returns>Collection names used by the migrations.</returns>
    public IEnumerable<string> GetCollectionNames(IEnumerable<IMigration> migrations, bool isUp)
    {
        var registry = FindGeneratedRegistry(_assembly);
        if (registry == null || registry.Count == 0)
        {
            return Enumerable.Empty<string>();
        }

        var migrationTypes = migrations.Select(m => m.GetType().FullName).ToHashSet(StringComparer.Ordinal);
        var matchingDescriptors = registry.Where(d => migrationTypes.Contains(d.MigrationType.FullName));

        return matchingDescriptors
            .SelectMany(d => isUp ? d.UpCollections : d.DownCollections)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Find the highest version of migrations.
    /// </summary>
    /// <returns>Highest version in semantic view.</returns>
    public Version GetNewestLocalVersion()
    {
        var migrations = GetAllMigrations();
        if (!migrations.Any())
        {
            return Version.Zero();
        }

        return migrations.Max(m => m.Version);
    }

    /// <summary>
    /// Find all migrations in specific assembly via reflection (fallback).
    /// </summary>
    /// <param name="assembly">Assembly with migrations classes.</param>
    /// <returns>List of all found migrations.</returns>
    private static List<IMigration> GetAllMigrationsViaReflection(Assembly assembly)
    {
        List<IMigration> result;
        try
        {
            result = assembly.GetTypes()
                .Where(type =>
                    typeof(IMigration).IsAssignableFrom(type)
                    && !type.IsAbstract
                    && type.GetCustomAttribute<IgnoreMigrationAttribute>() == null)
                .Select(Activator.CreateInstance)
                .OfType<IMigration>()
                .ToList();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Can't find migrations in assembly {assembly.FullName}", exception);
        }

        if (!result.Any())
        {
            throw new MigrationNotFoundException(assembly.FullName ?? string.Empty, null);
        }

        return result;
    }

    /// <summary>
    /// Finds the generated migration registry in the assembly.
    /// </summary>
    /// <param name="assembly">Assembly to search.</param>
    /// <returns>List of migration descriptors, or null if not found.</returns>
    private static IReadOnlyList<MigrationDescriptor>? FindGeneratedRegistry(Assembly? assembly)
    {
        if (assembly == null)
        {
            return null;
        }

        try
        {
            var registryType = assembly.GetTypes()
                .FirstOrDefault(t => t.GetCustomAttribute<GeneratedMigrationRegistryAttribute>() != null);

            if (registryType == null)
            {
                return null;
            }

            var method = registryType.GetMethod(nameof(GetAllMigrations), BindingFlags.Public | BindingFlags.Static);
            return method?.Invoke(null, null) as IReadOnlyList<MigrationDescriptor>;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

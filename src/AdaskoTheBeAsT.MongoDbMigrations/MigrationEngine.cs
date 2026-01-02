using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.MongoDbMigrations.Core;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using AdaskoTheBeAsT.MongoDbMigrations.Document;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using IMigration = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IMigration;
using MigrationContext = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.MigrationContext;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations;

/// <summary>
/// Migration engine for MongoDB database migrations.
/// Use <see cref="MigrationEngineBuilder"/> to create an instance.
/// </summary>
public sealed class MigrationEngine
    : ILocator,
        ISchemeValidation,
        IMigrationRunner,
        IDisposable
{
    private readonly IList<Action<InterimMigrationResult>> _progressHandlers = new List<Action<InterimMigrationResult>>();
    private readonly IList<Action<IMigration>> _beforeMigrationHandlers = new List<Action<IMigration>>();
    private readonly IList<Action<IMigration, bool>> _afterMigrationHandlers = new List<Action<IMigration, bool>>();

    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly MigrationManager _locator;
    private readonly DatabaseManager _status;
    private readonly bool _ownsClient;
    private readonly SshConfig? _sshConfig;

    private bool _schemeValidationNeeded;
    private CancellationToken _token;
    private bool _useTransaction;
    private bool _dryRun;
    private bool _disposed;

    static MigrationEngine()
    {
        BsonSerializer.RegisterSerializer(typeof(Version), new VersionSerializer());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationEngine"/> class.
    /// Use <see cref="MigrationEngineBuilder"/> to create instances.
    /// </summary>
    /// <param name="mongoClient">The MongoDB client.</param>
    /// <param name="database">The MongoDB database.</param>
    /// <param name="emulation">The MongoDB emulation type.</param>
    /// <param name="ownsClient">Whether this engine owns (and should dispose) the client.</param>
    /// <param name="sshConfig">Optional SSH configuration.</param>
    internal MigrationEngine(
        IMongoClient mongoClient,
        IMongoDatabase database,
        MongoEmulation emulation,
        bool ownsClient,
        SshConfig? sshConfig = null)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _ownsClient = ownsClient;
        _sshConfig = sshConfig;
        _locator = new MigrationManager();
        _status = new DatabaseManager(database, emulation);
    }

    /// <summary>
    /// Enables transaction support for the migration.
    /// </summary>
    /// <returns>The migration engine for chaining.</returns>
    public MigrationEngine UseTransaction()
    {
        _useTransaction = true;
        return this;
    }

    /// <inheritdoc/>
    public ISchemeValidation UseAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        _locator.SetAssembly(assembly);
        return this;
    }

    /// <inheritdoc/>
    public ISchemeValidation UseAssemblyOfType(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        _locator.LookInAssemblyOfType(type);
        return this;
    }

    /// <inheritdoc/>
    public ISchemeValidation UseAssemblyOfType<T>()
    {
        _locator.LookInAssemblyOfType<T>();
        return this;
    }

    /// <inheritdoc/>
#pragma warning disable IDE0060 // Remove unused parameter - kept for backwards API compatibility
    public IMigrationRunner UseSchemeValidation(bool enabled, string? migrationProjectLocation = null)
#pragma warning restore IDE0060
    {
        _schemeValidationNeeded = enabled;
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseProgressHandler(Action<InterimMigrationResult> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _progressHandlers.Add(action);
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseCancellationToken(CancellationToken token)
    {
        if (!token.CanBeCanceled)
        {
            throw new ArgumentException("Invalid token or it's canceled already.", nameof(token));
        }

        _token = token;
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseCustomSpecificationCollectionName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        _status.SpecCollectionName = name;
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseDryRun(bool enabled = true)
    {
        _dryRun = enabled;
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseBeforeMigration(Action<IMigration> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _beforeMigrationHandlers.Add(action);
        return this;
    }

    /// <inheritdoc/>
    public IMigrationRunner UseAfterMigration(Action<IMigration, bool> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _afterMigrationHandlers.Add(action);
        return this;
    }

    /// <inheritdoc/>
    public Task<MigrationResult> RunAsync(Version version, CancellationToken cancellationToken = default)
    {
        return RunInternalAsync(version, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MigrationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var targetVersion = _locator.GetNewestLocalVersion();
        return RunAsync(targetVersion, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MigrationResult> RollbackAsync(int stepsBack, CancellationToken cancellationToken = default)
    {
        if (stepsBack < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(stepsBack), "Steps back must be at least 1.");
        }

        var currentVersion = _status.GetVersion();
        var migrations = _locator.GetAllMigrations()
            .Where(m => m.Version <= currentVersion)
            .OrderByDescending(m => m.Version)
            .Take(stepsBack + 1)
            .ToList();

        if (migrations.Count <= stepsBack)
        {
            return RunAsync(Version.Zero(), cancellationToken);
        }

        var targetVersion = migrations[stepsBack].Version;
        return RunAsync(targetVersion, cancellationToken);
    }

    /// <summary>
    /// Disposes the migration engine and releases resources.
    /// Only disposes the MongoDB client if it was created internally (via connection string).
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

#pragma warning disable IDISP007 // Don't dispose injected - SSH config is owned by engine when created via builder
        if (_sshConfig?.SshClient.IsConnected == true)
        {
            _sshConfig.SshClient.Dispose();
            _sshConfig.ForwardedPortLocal.Dispose();
        }
#pragma warning restore IDISP007

        if (_ownsClient && _mongoClient is IDisposable disposableClient)
        {
            disposableClient.Dispose();
        }
    }

#pragma warning disable MA0051 // Method is too long
    private async Task<MigrationResult> RunInternalAsync(Version version, CancellationToken cancellationToken)
    {
#pragma warning restore MA0051
        var effectiveToken = _token.CanBeCanceled ? _token : cancellationToken;

        var currentDatabaseVersion = _status.GetVersion();
        var migrations = _locator.GetMigrations(currentDatabaseVersion, version);

        var result = new MigrationResult
        {
            ServerAddress = string.Join(",", _database.Client.Settings.Servers),
            DatabaseName = _database.DatabaseNamespace.DatabaseName,
            Success = true,
            IsDryRun = _dryRun,
        };

        if (migrations.Count == 0)
        {
            result.CurrentVersion = currentDatabaseVersion;
            return result;
        }

        effectiveToken.ThrowIfCancellationRequested();

        var isUp = version > currentDatabaseVersion;

        if (_schemeValidationNeeded)
        {
            var collectionNames = _locator.GetCollectionNames(migrations, isUp);
            if (collectionNames.Any())
            {
                var validationResult = await MongoSchemeValidator.ValidateAsync(
                    collectionNames,
                    _database,
                    effectiveToken).ConfigureAwait(false);

                if (validationResult.FailedCollections.Any())
                {
                    result.Success = false;
                    var failedCollections = string.Join(Environment.NewLine, validationResult.FailedCollections);
                    throw new InvalidOperationException(
                        $"Some schema validation issues found in: {failedCollections}");
                }
            }
        }

        var counter = 0;
        var session = _useTransaction
            ? (IMigrationSession)new MongoMigrationSession(_database.Client)
            : new SimpleMigrationSession();

        using (var transaction = session.BeginTransaction())
        {
            var context = new MigrationContext(_database, transaction.Session, effectiveToken);
            foreach (var m in migrations)
            {
                effectiveToken.ThrowIfCancellationRequested();

                counter++;
                var increment = new InterimMigrationResult();
                var migrationSuccess = true;

                try
                {
                    foreach (var beforeHandler in _beforeMigrationHandlers)
                    {
                        beforeHandler(m);
                    }

                    if (!_dryRun)
                    {
                        if (isUp)
                        {
                            await m.UpAsync(context).ConfigureAwait(false);
                        }
                        else
                        {
                            await m.DownAsync(context).ConfigureAwait(false);
                        }

                        var insertedMigration = _status.SaveMigration(m, isUp, transaction.Session);
                        increment.MigrationName = insertedMigration.Name;
                        increment.TargetVersion = insertedMigration.Ver;
                    }
                    else
                    {
                        increment.MigrationName = m.Name;
                        increment.TargetVersion = m.Version;
                    }

                    increment.ServerAddress = result.ServerAddress;
                    increment.DatabaseName = result.DatabaseName;
                    increment.CurrentNumber = counter;
                    increment.TotalCount = migrations.Count;
                    result.InterimSteps.Add(increment);
                }
                catch (Exception ex)
                {
                    result.Success = false;
#pragma warning disable S1854 // False positive - migrationSuccess is used in finally block which runs before exception propagates
                    migrationSuccess = false;
#pragma warning restore S1854
                    throw new InvalidOperationException("Something went wrong during migration", ex);
                }
                finally
                {
                    foreach (var afterHandler in _afterMigrationHandlers)
                    {
                        afterHandler(m, migrationSuccess);
                    }

                    foreach (var action in _progressHandlers)
                    {
                        action(increment);
                    }

                    if (!_dryRun)
                    {
                        result.CurrentVersion = _status.GetVersion();
                    }
                    else
                    {
                        result.CurrentVersion = version;
                    }
                }
            }

            if (!_dryRun)
            {
                transaction.Commit(effectiveToken);
            }
        }

        return result;
    }
}

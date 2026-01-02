using System;
using System.Linq;
using System.Reflection;
using AdaskoTheBeAsT.MongoDbMigrations.Document;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using IMigration = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IMigration;
using Version = AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// Works with applied migrations.
/// </summary>
public class DatabaseManager
{
    private const string SpecificationCollectionDefaultName = "_migrations";
    private readonly IMongoDatabase _database;
    private string _specCollectionName = string.Empty;

    public DatabaseManager(IMongoDatabase database, MongoEmulation emulation)
    {
        _database = database ?? throw new TypeInitializationException("Database can't be null", null);
        var isInitial = false;
        using var collectionNameList = _database.ListCollectionNames();
        if (!collectionNameList.ToList().Contains(SpecCollectionName, StringComparer.Ordinal))
        {
            _database.CreateCollection(SpecCollectionName);
            isInitial = true;
        }

        switch (emulation)
        {
            case MongoEmulation.AzureCosmos when !IsAzureCosmosDbCompatible(isInitial):
                throw new InvalidOperationException(
                    $"""
                     Your current setup isn't ready for this migration run.
                     Please create an ascending index to the filed '{typeof(SpecificationItem).GetProperty(
                         nameof(SpecificationItem.ApplyingDateTime),
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? string.Empty}'
                     at collection '{SpecCollectionName}' manually and retry the migration run. Be aware that indexing may take some time.
                     """);
            default:
                return;
        }
    }

    public string SpecCollectionName
    {
        get
        {
            return string.IsNullOrEmpty(_specCollectionName) ? SpecificationCollectionDefaultName : _specCollectionName;
        }

        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _specCollectionName = value;
            }
        }
    }

    /// <summary>
    /// Return database version based on last applied migration.
    /// </summary>
    /// <returns>Database version in semantic view.</returns>
    public Version GetVersion()
    {
        var lastMigration = GetLastAppliedMigration();
        if (lastMigration == null || lastMigration.IsUp)
        {
            return lastMigration?.Ver ?? Version.Zero();
        }

        var migration = GetAppliedMigrations()
            .Find(item => item.IsUp && item.Ver < lastMigration.Ver)
            .Sort(Builders<SpecificationItem>.Sort.Descending(x => x.ApplyingDateTime))
            .FirstOrDefault();

        return migration?.Ver ?? Version.Zero();
    }

    /// <summary>
    /// Find last applied migration by applying date and time.
    /// </summary>
    /// <returns>Applied migration.</returns>
    public SpecificationItem GetLastAppliedMigration()
    {
        return GetAppliedMigrations()
            .Find(FilterDefinition<SpecificationItem>.Empty)
            .Sort(Builders<SpecificationItem>.Sort.Descending(x => x.ApplyingDateTime))
            .FirstOrDefault();
    }

    /// <summary>
    /// Commit migration to the database.
    /// </summary>
    /// <param name="migration">Migration instance.</param>
    /// <param name="isUp">True if roll forward otherwise roll back.</param>
    /// <param name="transaction">Client session handle for transaction support.</param>
    /// <returns>Applied migration.</returns>
    internal SpecificationItem SaveMigration(IMigration migration, bool isUp, IClientSessionHandle? transaction)
    {
        var appliedMigration = new SpecificationItem
        {
            Name = migration.Name,
            Ver = migration.Version,
            IsUp = isUp,
            ApplyingDateTime = DateTime.UtcNow,
        };

        if (transaction != null)
        {
            GetAppliedMigrations().InsertOne(transaction, appliedMigration);
        }
        else
        {
            GetAppliedMigrations().InsertOne(appliedMigration);
        }

        return appliedMigration;
    }

    private bool IsAzureCosmosDbCompatible(bool isInitial)
    {
        if (_database == null)
        {
            throw new TypeInitializationException(nameof(DatabaseManager), new System.Exception($"{nameof(_database)} hasn't been initialized."));
        }

        // If it's a fist migration run and there are no records in the _migrations collection.
        if (isInitial)
        {
            // Just create an index
            var indexOptions = new CreateIndexOptions<SpecificationItem>();
            var indexKey = Builders<SpecificationItem>.IndexKeys.Ascending(x => x.ApplyingDateTime);
            var indexModel = new CreateIndexModel<SpecificationItem>(indexKey, indexOptions);
            var collection = _database.GetCollection<SpecificationItem>(SpecCollectionName);
            collection.Indexes.CreateOne(indexModel);
            return true;
        }

        // Check that index exists and return true, otherwise false.
        using var defaultCollectionCursor = _database.GetCollection<SpecificationItem>(SpecCollectionName).Indexes.List();
        var indexes = defaultCollectionCursor.ToList();
        var targetIndex = typeof(SpecificationItem)
            .GetProperty(
                nameof(SpecificationItem.ApplyingDateTime),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            ?.GetCustomAttribute<BsonElementAttribute>()
            ?.ElementName;
        if (string.IsNullOrEmpty(targetIndex))
        {
            return false;
        }

        return indexes.Any(x => x.GetValue("name").ToString()?.StartsWith(targetIndex, StringComparison.Ordinal) == true);
    }

    private IMongoCollection<SpecificationItem> GetAppliedMigrations()
    {
        return _database.GetCollection<SpecificationItem>(SpecCollectionName);
    }
}

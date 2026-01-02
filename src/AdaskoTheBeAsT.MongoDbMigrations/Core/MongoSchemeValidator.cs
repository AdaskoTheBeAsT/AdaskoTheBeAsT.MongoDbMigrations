using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core;

/// <summary>
/// Validates MongoDB schema during migrations.
/// </summary>
public static class MongoSchemeValidator
{
    /// <summary>
    /// This method check documents which will be affected by migration. For successful result all
    /// documents in collection must have the same scheme otherwise validation will be failed.
    /// </summary>
    /// <param name="collectionNames">Collection names to validate.</param>
    /// <param name="database">Instance of mongo database.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public static Task<SchemeValidationResult> ValidateAsync(
        IEnumerable<string> collectionNames,
        IMongoDatabase database,
        CancellationToken cancellationToken = default)
    {
        if (collectionNames == null)
        {
            throw new ArgumentNullException(nameof(collectionNames));
        }

        if (database == null)
        {
            throw new ArgumentNullException(nameof(database));
        }

        return CheckAsync(database, collectionNames, cancellationToken);
    }

    private static async Task<SchemeValidationResult> CheckAsync(
        IMongoDatabase database,
        IEnumerable<string> names,
        CancellationToken cancellationToken)
    {
        var result = new SchemeValidationResult();
        foreach (var name in names.Distinct(StringComparer.Ordinal))
        {
            var isFailed = false;
            var collection = database.GetCollection<BsonDocument>(name);

            if (collection == null || await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty, cancellationToken: cancellationToken).ConfigureAwait(false) == 0)
            {
                continue;
            }

            var doc = await collection.Find(FilterDefinition<BsonDocument>.Empty)
                .FirstAsync(cancellationToken).ConfigureAwait(false);

            var refScheme = doc.Elements.ToDictionary(i => i.Name, i => i.Value.BsonType, StringComparer.Ordinal);

            using var cursor = await collection.Find(FilterDefinition<BsonDocument>.Empty).ToCursorAsync(cancellationToken).ConfigureAwait(false);
            while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                if (isFailed)
                {
                    break;
                }

                var batch = cursor.Current;
                foreach (var document in batch)
                {
                    isFailed = !document.Elements.ToDictionary(i => i.Name, i => i.Value.BsonType, StringComparer.Ordinal).SequenceEqual(refScheme);
                }
            }

            result.Add(name, isFailed);
        }

        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaskoTheBeAsT.MongoDbMigrations.Core.Contracts;

public class SchemeValidationResult
{
    private readonly IList<string> _validCollections = new List<string>();
    private readonly IList<string> _invalidCollections = new List<string>();

    public IEnumerable<string> ValidCollections => _validCollections.Distinct(StringComparer.Ordinal);

    public IEnumerable<string> FailedCollections => _invalidCollections.Distinct(StringComparer.Ordinal);

    public void Add(string name, bool isFailed)
    {
        if (isFailed)
        {
            _invalidCollections.Add(name);
        }
        else
        {
            _validCollections.Add(name);
        }
    }
}

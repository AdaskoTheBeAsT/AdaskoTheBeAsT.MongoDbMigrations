using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators.Test;

public class CollectionNameExtractorTests
{
    [Fact]
    public void Generator_WithGetCollectionCall_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>("users");
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"users\"");
    }

    [Fact]
    public void Generator_WithMultipleCollections_ShouldExtractAllCollectionNames()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public async Task UpAsync(MigrationContext context)
                {
                    var users = context.Database.GetCollection<BsonDocument>("users");
                    var orders = context.Database.GetCollection<BsonDocument>("orders");
                    await Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"users\"");
        generatedCode.Should().Contain("\"orders\"");
    }

    [Fact]
    public void Generator_WithCollectionsInBothMethods_ShouldExtractSeparately()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>("upCollection");
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>("downCollection");
                    return Task.CompletedTask;
                }
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"upCollection\"");
        generatedCode.Should().Contain("\"downCollection\"");
    }

    [Fact]
    public void Generator_WithCreateCollectionCall_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public async Task UpAsync(MigrationContext context)
                {
                    await context.Database.CreateCollectionAsync("newCollection");
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"newCollection\"");
    }

    [Fact]
    public void Generator_WithDropCollectionCall_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                
                public async Task DownAsync(MigrationContext context)
                {
                    await context.Database.DropCollectionAsync("oldCollection");
                }
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"oldCollection\"");
    }

    [Fact]
    public void Generator_WithRenameCollectionCall_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public async Task UpAsync(MigrationContext context)
                {
                    await context.Database.RenameCollectionAsync("oldName", "newName");
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"oldName\"");
    }

    [Fact]
    public void Generator_WithConstantCollectionName_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                private const string CollectionName = "constantCollection";
                
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>(CollectionName);
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"constantCollection\"");
    }

    [Fact]
    public void Generator_WithNameofExpression_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class Users { }
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>(nameof(Users));
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"Users\"");
    }

    [Fact]
    public void Generator_WithNameofMemberAccess_ShouldExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class Entities
            {
                public static string Orders { get; } = "orders";
            }
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collection = context.Database.GetCollection<BsonDocument>(nameof(Entities.Orders));
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"Orders\"");
    }

    [Fact]
    public void Generator_WithDuplicateCollectionNames_ShouldDeduplicateInOutput()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public async Task UpAsync(MigrationContext context)
                {
                    var col1 = context.Database.GetCollection<BsonDocument>("users");
                    var col2 = context.Database.GetCollection<BsonDocument>("users");
                    await Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        var usersCount = CountOccurrences(generatedCode, "\"users\"");
        usersCount.Should().Be(1);
    }

    [Fact]
    public void Generator_WithNoCollections_ShouldGenerateEmptyArrays()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("Array.Empty<string>()");
    }

    [Fact]
    public void Generator_WithNonMongoMethod_ShouldNotExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var name = GetCollection("notMongo");
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
                
                private string GetCollection(string name) => name;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().NotContain("\"notMongo\"");
        generatedCode.Should().Contain("Array.Empty<string>()");
    }

    [Fact]
    public void Generator_WithMissingUpMethod_ShouldStillGenerateRegistry()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);
    }

    [Fact]
    public void Generator_WithListCollectionNamesAsync_ShouldNotExtractCollectionName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public async Task UpAsync(MigrationContext context)
                {
                    var names = await context.Database.ListCollectionNamesAsync();
                    await Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("Array.Empty<string>()");
    }

    [Fact]
    public void Generator_WithDynamicCollectionName_ShouldNotExtractName()
    {
        var source = """
            using System.Threading.Tasks;
            using MongoDB.Bson;
            using MongoDB.Driver;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context)
                {
                    var collectionName = GetDynamicName();
                    var collection = context.Database.GetCollection<BsonDocument>(collectionName);
                    return Task.CompletedTask;
                }
                
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
                
                private string GetDynamicName() => "dynamic_" + System.DateTime.Now.Ticks;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("Array.Empty<string>()");
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<SyntaxTree> GeneratedTrees) RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Abstractions.IMigration).Assembly.Location),
        };

        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Threading.Tasks.dll")));

        var mongoDriverAssembly = typeof(MongoDB.Driver.IMongoDatabase).Assembly;
        references.Add(MetadataReference.CreateFromFile(mongoDriverAssembly.Location));

        var mongoBsonAssembly = typeof(MongoDB.Bson.BsonDocument).Assembly;
        references.Add(MetadataReference.CreateFromFile(mongoBsonAssembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new MigrationSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        _ = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var generatedTrees = outputCompilation.SyntaxTrees
            .Where(t => t.FilePath.EndsWith(".g.cs", StringComparison.Ordinal))
            .ToImmutableArray();

        return (diagnostics, generatedTrees);
    }
}

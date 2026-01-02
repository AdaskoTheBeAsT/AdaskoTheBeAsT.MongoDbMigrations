using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators.Test;

public class MigrationSourceGeneratorTests
{
    [Fact]
    public void Generator_WithNoMigrations_ShouldNotGenerateCode()
    {
        var source = """
            namespace TestApp;
            
            public class NotAMigration
            {
                public void DoSomething() { }
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_WithValidMigration_ShouldGenerateRegistry()
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
        generatedCode.Should().Contain("MigrationRegistry");
        generatedCode.Should().Contain("TestApp.TestMigration");
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(1, 0, 0)");
    }

    [Fact]
    public void Generator_WithIgnoredMigration_ShouldNotIncludeInRegistry()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            [IgnoreMigration]
            public class IgnoredMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Ignored Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_WithAbstractMigration_ShouldNotIncludeInRegistry()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public abstract class BaseMigration : IMigration
            {
                public abstract Version Version { get; }
                public abstract string Name { get; }
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_WithDuplicateVersions_ShouldReportDiagnostic()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class Migration1 : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Migration 1";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            
            public class Migration2 : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Migration 2";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        diagnostics.Should().NotBeEmpty();
        diagnostics.Should().Contain(d => d.Id == "MONGO002");
    }

    [Fact]
    public void Generator_WithVersionFromStringConstructor_ShouldParseCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version("2.5.3");
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(2, 5, 3)");
    }

    [Fact]
    public void Generator_WithMultipleMigrations_ShouldOrderByVersion()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class Migration2 : IMigration
            {
                public Version Version => new Version(2, 0, 0);
                public string Name => "Migration 2";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            
            public class Migration1 : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Migration 1";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        var migration1Index = generatedCode.IndexOf("Migration1", StringComparison.Ordinal);
        var migration2Index = generatedCode.IndexOf("Migration2", StringComparison.Ordinal);

        migration1Index.Should().BeLessThan(migration2Index);
    }

    [Fact]
    public void Generator_WithVersionFromInitializer_ShouldParseCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version { get; } = new Version(3, 2, 1);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(3, 2, 1)");
    }

    [Fact]
    public void Generator_WithVersionFromGetterWithReturn_ShouldParseCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version
                {
                    get { return new Version(4, 5, 6); }
                }
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(4, 5, 6)");
    }

    [Fact]
    public void Generator_WithVersionFromGetterExpressionBody_ShouldParseCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version
                {
                    get => new Version(7, 8, 9);
                }
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(7, 8, 9)");
    }

    [Fact]
    public void Generator_WithImplicitVersionCreation_ShouldParseCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new(1, 2, 3);
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version(1, 2, 3)");
    }

    [Fact]
    public void Generator_WithNameContainingSpecialChars_ShouldEscapeCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => "Test \"Migration\" with\\backslash";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\\\"Migration\\\"");
        generatedCode.Should().Contain("\\\\backslash");
    }

    [Fact]
    public void Generator_WithNoNameProperty_ShouldUseClassName()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class CustomMigrationClassName : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name => GetName();
                
                private string GetName() => "dynamic";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"CustomMigrationClassName\"");
    }

    [Fact]
    public void Generator_WithNameFromInitializer_ShouldExtractCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name { get; } = "Initialized Name";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"Initialized Name\"");
    }

    [Fact]
    public void Generator_WithNameFromGetterReturn_ShouldExtractCorrectly()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version(1, 0, 0);
                public string Name
                {
                    get { return "Getter Return Name"; }
                }
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().HaveCount(1);

        var generatedCode = generatedTrees[0].ToString();
        generatedCode.Should().Contain("\"Getter Return Name\"");
    }

    [Fact]
    public void Generator_WithInvalidVersionString_ShouldNotGenerateCode()
    {
        var source = """
            using System.Threading.Tasks;
            using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;
            
            namespace TestApp;
            
            public class TestMigration : IMigration
            {
                public Version Version => new Version("invalid");
                public string Name => "Test Migration";
                
                public Task UpAsync(MigrationContext context) => Task.CompletedTask;
                public Task DownAsync(MigrationContext context) => Task.CompletedTask;
            }
            """;

        var (_, generatedTrees) = RunGenerator(source);

        generatedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Generator_WithClassNotImplementingIMigration_ShouldNotGenerate()
    {
        var source = """
            using System.Threading.Tasks;
            
            namespace TestApp;
            
            public interface IOtherInterface { }
            
            public class TestClass : IOtherInterface
            {
                public string Version => "1.0.0";
            }
            """;

        var (diagnostics, generatedTrees) = RunGenerator(source);

        diagnostics.Should().BeEmpty();
        generatedTrees.Should().BeEmpty();
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

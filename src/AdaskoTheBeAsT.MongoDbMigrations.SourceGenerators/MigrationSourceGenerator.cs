using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators;

/// <summary>
/// Source generator that creates a migration registry at compile time.
/// </summary>
[Generator]
public class MigrationSourceGenerator : IIncrementalGenerator
{
    private const string IMigrationFullName = "AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IMigration";
    private const string IgnoreMigrationAttributeName = "AdaskoTheBeAsT.MongoDbMigrations.Abstractions.IgnoreMigrationAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var migrationClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsMigrationCandidate(node),
                transform: static (ctx, _) => GetMigrationInfo(ctx))
            .Where(static m => m is not null);

        var compilationAndMigrations = context.CompilationProvider
            .Combine(migrationClasses.Collect());

        context.RegisterSourceOutput(
            compilationAndMigrations,
            static (spc, source) => Execute(source.Right!, spc));
    }

    private static bool IsMigrationCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
               && !classDecl.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AbstractKeyword)
               && classDecl.BaseList?.Types.Count > 0;
    }

    private static MigrationInfo? GetMigrationInfo(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        if (!ImplementsIMigration(classSymbol))
        {
            return null;
        }

        if (HasIgnoreMigrationAttribute(classSymbol))
        {
            return null;
        }

        var versionInfo = ExtractVersionInfo(classSymbol);
        if (versionInfo is null)
        {
            return null;
        }

        var nameInfo = ExtractNameInfo(classSymbol);

        var upCollections = CollectionNameExtractor.ExtractFromMethod(classDecl, semanticModel, "UpAsync");
        var downCollections = CollectionNameExtractor.ExtractFromMethod(classDecl, semanticModel, "DownAsync");

        return new MigrationInfo(
            classSymbol.ToDisplayString(),
            versionInfo.Value,
            nameInfo,
            upCollections.ToList(),
            downCollections.ToList());
    }

    private static bool ImplementsIMigration(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.AllInterfaces.Any(item => string.Equals(item.ToDisplayString(), IMigrationFullName, StringComparison.Ordinal)))
        {
            return true;
        }

        return false;
    }

    private static bool HasIgnoreMigrationAttribute(INamedTypeSymbol classSymbol)
    {
        if (classSymbol.GetAttributes().Any(item => string.Equals(
                item.AttributeClass?.ToDisplayString(),
                IgnoreMigrationAttributeName,
                StringComparison.Ordinal)))
        {
            return true;
        }

        return false;
    }

    private static VersionInfo? ExtractVersionInfo(INamedTypeSymbol classSymbol)
    {
        var versionProperty = classSymbol.GetMembers("Version")
            .OfType<IPropertySymbol>()
            .FirstOrDefault();

        if (versionProperty is null)
        {
            return null;
        }

        foreach (var syntaxRef in versionProperty.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propDecl)
            {
                var version = ExtractVersionFromProperty(propDecl);
                if (version is not null)
                {
                    return version;
                }
            }
        }

        return null;
    }

    private static VersionInfo? ExtractVersionFromProperty(PropertyDeclarationSyntax propDecl)
    {
        ExpressionSyntax? expression = null;

        if (propDecl.ExpressionBody is not null)
        {
            expression = propDecl.ExpressionBody.Expression;
        }
        else if (propDecl.Initializer is not null)
        {
            expression = propDecl.Initializer.Value;
        }
        else
        {
            var getter = propDecl.AccessorList?.Accessors
                .FirstOrDefault(a => a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration));

            if (getter?.ExpressionBody is not null)
            {
                expression = getter.ExpressionBody.Expression;
            }
            else if (getter?.Body is not null)
            {
                var returnStatement = getter.Body.Statements
                    .OfType<ReturnStatementSyntax>()
                    .FirstOrDefault();
                expression = returnStatement?.Expression;
            }
        }

        if (expression is null)
        {
            return null;
        }

        return ParseVersionExpression(expression);
    }

    private static VersionInfo? ParseVersionExpression(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal &&
            literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
        {
            var versionString = literal.Token.ValueText;
            return ParseVersionString(versionString);
        }

        if (expression is ObjectCreationExpressionSyntax objectCreation)
        {
            return ParseVersionFromObjectCreation(objectCreation);
        }

        if (expression is ImplicitObjectCreationExpressionSyntax implicitCreation)
        {
            return ParseVersionFromArgumentList(implicitCreation.ArgumentList);
        }

        return null;
    }

    private static VersionInfo? ParseVersionFromObjectCreation(ObjectCreationExpressionSyntax objectCreation)
    {
        if (objectCreation.ArgumentList is null)
        {
            return null;
        }

        var args = objectCreation.ArgumentList.Arguments;

        if (args.Count == 1 &&
            args[0].Expression is LiteralExpressionSyntax stringLiteral &&
            stringLiteral.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
        {
            return ParseVersionString(stringLiteral.Token.ValueText);
        }

        return ParseVersionFromArgumentList(objectCreation.ArgumentList);
    }

    private static VersionInfo? ParseVersionFromArgumentList(ArgumentListSyntax? argumentList)
    {
        if (argumentList is null)
        {
            return null;
        }

        var args = argumentList.Arguments;
        if (args.Count != 3)
        {
            return null;
        }

        if (!TryGetIntLiteral(args[0].Expression, out var major) ||
            !TryGetIntLiteral(args[1].Expression, out var minor) ||
            !TryGetIntLiteral(args[2].Expression, out var revision))
        {
            return null;
        }

        return new VersionInfo(major, minor, revision);
    }

    private static bool TryGetIntLiteral(ExpressionSyntax expression, out int value)
    {
        value = 0;
        if (expression is LiteralExpressionSyntax literal &&
            literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression) &&
            literal.Token.Value is int intValue)
        {
            value = intValue;
            return true;
        }

        return false;
    }

    private static VersionInfo? ParseVersionString(string versionString)
    {
        var parts = versionString.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor) ||
            !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var revision))
        {
            return null;
        }

        return new VersionInfo(major, minor, revision);
    }

    private static string ExtractNameInfo(INamedTypeSymbol classSymbol)
    {
        var nameProperty = classSymbol.GetMembers("Name")
            .OfType<IPropertySymbol>()
            .FirstOrDefault();

        if (nameProperty is null)
        {
            return classSymbol.Name;
        }

        foreach (var syntaxRef in nameProperty.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propDecl)
            {
                var name = ExtractStringFromProperty(propDecl);
                if (name is not null)
                {
                    return name;
                }
            }
        }

        return classSymbol.Name;
    }

    private static string? ExtractStringFromProperty(PropertyDeclarationSyntax propDecl)
    {
        ExpressionSyntax? expression = null;

        if (propDecl.ExpressionBody is not null)
        {
            expression = propDecl.ExpressionBody.Expression;
        }
        else if (propDecl.Initializer is not null)
        {
            expression = propDecl.Initializer.Value;
        }
        else
        {
            var getter = propDecl.AccessorList?.Accessors
                .FirstOrDefault(a => a.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration));

            if (getter?.ExpressionBody is not null)
            {
                expression = getter.ExpressionBody.Expression;
            }
            else if (getter?.Body is not null)
            {
                var returnStatement = getter.Body.Statements
                    .OfType<ReturnStatementSyntax>()
                    .FirstOrDefault();
                expression = returnStatement?.Expression;
            }
        }

        if (expression is LiteralExpressionSyntax literal &&
            literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        return null;
    }

    private static void Execute(
        ImmutableArray<MigrationInfo?> migrations,
        SourceProductionContext context)
    {
        var validMigrations = migrations
            .Where(m => m is not null)
            .Cast<MigrationInfo>()
            .OrderBy(m => m.Version)
            .ToList();

        if (validMigrations.Count == 0)
        {
            return;
        }

        var duplicates = validMigrations
            .GroupBy(m => m.Version)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var duplicate in duplicates)
        {
            var migrationNames = string.Join(", ", duplicate.Select(m => m.FullTypeName));
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.DuplicateMigrationVersion,
                Location.None,
                duplicate.Key,
                migrationNames));
        }

        var source = GenerateRegistrySource(validMigrations);
        context.AddSource("MigrationRegistry.g.cs", SourceText.From(source, Encoding.UTF8));
    }

#pragma warning disable MA0051 // Method is too long
    private static string GenerateRegistrySource(List<MigrationInfo> migrations)
#pragma warning restore MA0051 // Method is too long
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using AdaskoTheBeAsT.MongoDbMigrations.Abstractions;");
        sb.AppendLine();
        sb.AppendLine("namespace AdaskoTheBeAsT.MongoDbMigrations.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated migration registry.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("[GeneratedMigrationRegistry]");
        sb.AppendLine("public static class MigrationRegistry");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly MigrationDescriptor[] Migrations = new MigrationDescriptor[]");
        sb.AppendLine("    {");

        for (var i = 0; i < migrations.Count; i++)
        {
            var m = migrations[i];
            var upCollections = FormatStringArray(m.UpCollections);
            var downCollections = FormatStringArray(m.DownCollections);
            var comma = i < migrations.Count - 1 ? "," : string.Empty;

            sb.AppendLine($"        new MigrationDescriptor(");
            sb.AppendLine($"            typeof({m.FullTypeName}),");
            sb.AppendLine($"            new AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Version({m.Version.Major}, {m.Version.Minor}, {m.Version.Revision}),");
            sb.AppendLine($"            \"{EscapeString(m.Name)}\",");
            sb.AppendLine($"            {upCollections},");
            sb.AppendLine($"            {downCollections}){comma}");
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets all migrations in version order.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns>List of migration descriptors.</returns>");
        sb.AppendLine("    public static IReadOnlyList<MigrationDescriptor> GetAllMigrations()");
        sb.AppendLine("    {");
        sb.AppendLine("        return Migrations;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string FormatStringArray(IReadOnlyList<string> items)
    {
        if (items.Count == 0)
        {
            return "Array.Empty<string>()";
        }

        var escaped = items.Select(s => $"\"{EscapeString(s)}\"");
        return $"new[] {{ {string.Join(", ", escaped)} }}";
    }

    private static string EscapeString(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

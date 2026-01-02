using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators;

/// <summary>
/// Extracts MongoDB collection names from migration methods.
/// </summary>
internal static class CollectionNameExtractor
{
    private static readonly HashSet<string> CollectionMethods = new(StringComparer.Ordinal)
    {
        "GetCollection",
        "CreateCollection",
        "CreateCollectionAsync",
        "DropCollection",
        "DropCollectionAsync",
        "RenameCollection",
        "RenameCollectionAsync",
        "ListCollectionNames",
        "ListCollectionNamesAsync",
        "ListCollections",
        "ListCollectionsAsync",
    };

    public static IReadOnlyList<string> ExtractFromMethod(
        ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel,
        string methodName)
    {
        var method = classDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => string.Equals(m.Identifier.ValueText, methodName, StringComparison.Ordinal));

        if (method is null)
        {
            return Array.Empty<string>();
        }

        var collections = new List<string>();

        var invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol is null)
            {
                continue;
            }

            if (!CollectionMethods.Contains(methodSymbol.Name))
            {
                continue;
            }

            if (!IsMongoDatabase(methodSymbol.ContainingType))
            {
                continue;
            }

            var collectionName = ExtractCollectionNameFromInvocation(invocation, semanticModel);
            if (collectionName is not null && !collections.Contains(collectionName, StringComparer.OrdinalIgnoreCase))
            {
                collections.Add(collectionName);
            }
        }

        return collections;
    }

    private static bool IsMongoDatabase(INamedTypeSymbol? type)
    {
        if (type is null)
        {
            return false;
        }

        if (string.Equals(type.Name, "IMongoDatabase", StringComparison.Ordinal) &&
            string.Equals(type.ContainingNamespace.ToDisplayString(), "MongoDB.Driver", StringComparison.Ordinal))
        {
            return true;
        }

        if (type.AllInterfaces.Any(IsMongoDatabase))
        {
            return true;
        }

        if (type.BaseType is not null)
        {
            return IsMongoDatabase(type.BaseType);
        }

        return false;
    }

    private static string? ExtractCollectionNameFromInvocation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return null;
        }

        var firstArg = invocation.ArgumentList.Arguments[0].Expression;

        if (firstArg is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        var constantValue = semanticModel.GetConstantValue(firstArg);
        if (constantValue.HasValue && constantValue.Value is string str)
        {
            return str;
        }

        if (firstArg is InvocationExpressionSyntax nameofInvocation &&
            nameofInvocation.Expression is IdentifierNameSyntax identifier &&
            string.Equals(identifier.Identifier.ValueText, "nameof", StringComparison.OrdinalIgnoreCase) &&
            nameofInvocation.ArgumentList.Arguments.Count == 1)
        {
            var nameofArg = nameofInvocation.ArgumentList.Arguments[0].Expression;
            return GetLastIdentifier(nameofArg);
        }

        return null;
    }

    private static string? GetLastIdentifier(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null,
        };
    }
}

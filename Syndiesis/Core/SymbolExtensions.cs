using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using Serilog;
using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;
using System.Xml;

namespace Syndiesis.Core;

public static class SymbolExtensions
{
    public static bool IsEnumField(this ISymbol symbol)
    {
        return symbol is IFieldSymbol
        {
            ContainingSymbol: INamedTypeSymbol { TypeKind: TypeKind.Enum }
        };
    }

    public static bool IsRequired(this ISymbol symbol)
    {
        return symbol
            is IFieldSymbol { IsRequired: true }
            or IPropertySymbol { IsRequired: true }
            ;
    }

    public static bool IsConstant(this ISymbol symbol)
    {
        return symbol
            is IFieldSymbol { IsConst: true }
            or ILocalSymbol { IsConst: true }
            ;
    }

    public static bool IsRef(this ISymbol symbol)
    {
        return symbol
            is IFieldSymbol { RefKind: RefKind.Ref }
            or ILocalSymbol { RefKind: RefKind.Ref }
            or IParameterSymbol { RefKind: RefKind.Ref }
            or IMethodSymbol { RefKind: RefKind.Ref }
            or IPropertySymbol { RefKind: RefKind.Ref }
            ;
    }

    public static bool IsRefReadOnly(this ISymbol symbol)
    {
        return symbol
            is IFieldSymbol { RefKind: RefKind.RefReadOnly }
            or ILocalSymbol { RefKind: RefKind.RefReadOnly }
            or IParameterSymbol
            {
                RefKind: RefKind.RefReadOnly
                    or RefKind.RefReadOnlyParameter
            }
            or IMethodSymbol { RefKind: RefKind.RefReadOnly }
            or IPropertySymbol { RefKind: RefKind.RefReadOnly }
            ;
    }

    public static bool IsReadOnly(this ISymbol symbol)
    {
        return symbol
            is ITypeSymbol { IsReadOnly: true }
            or IFieldSymbol { IsReadOnly: true }
            or IPropertySymbol { IsReadOnly: true }
            or IMethodSymbol { IsReadOnly: true }
            ;
    }

    public static bool IsOperator(this IMethodSymbol method)
    {
        return method.MethodKind
            is MethodKind.BuiltinOperator
            or MethodKind.UserDefinedOperator
            ;
    }

    public static XmlDocument? GetDocumentationXmlDocument(
        this ISymbol symbol,
        CultureInfo? preferredCulture = null,
        bool expandIncludes = false,
        CancellationToken cancellationToken = default)
    {
        var xml = symbol.GetDocumentationCommentXml(
            preferredCulture, expandIncludes, cancellationToken);
        if (string.IsNullOrEmpty(xml))
            return null;
        return CreateXmlDocument(xml);
    }

    private static XmlDocument CreateXmlDocument(string xml)
    {
        var document = new XmlDocument();
        try
        {
            document.Load(xml);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load the XML document");
        }
        return document;
    }

    public static bool HasFileAccessibility(this ISymbol symbol)
    {
        return symbol
            is INamedTypeSymbol { IsFileLocal: true };
    }
    
    // known unspeakable characters on compiler-generated names
    private static readonly SearchValues<char> _unspeakableChars = SearchValues.Create("<>$#");

    public static bool IsUnspeakableName(string name)
    {
        return name.AsSpan().ContainsAny(_unspeakableChars);
    }
    
    public static bool HasUnspeakableName(this ISymbol symbol)
    {
        return IsUnspeakableName(symbol.Name);
    }

    // TODO: Evaluate if this is even necessary
    public static ReadOnlySpan<char> GetNameWithoutGenericSuffix(this ISymbol symbol)
    {
        var nameSpan = symbol.Name.AsSpan();
        nameSpan.SplitOnce('`', out var name, out _);
        return name;
    }

    public static ImmutableArray<IFieldSymbol> GetFields(this ITypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IFieldSymbol>()
            .ToImmutableArray();
    }

    public static ImmutableArray<IPropertySymbol> GetProperties(this ITypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToImmutableArray();
    }

    public static IEnumerable<string> YieldNamespaceIdentifiers(this INamespaceSymbol @namespace)
    {
        var current = @namespace;
        while (true)
        {
            if (current.IsGlobalNamespace)
            {
                yield break;
            }

            yield return current.Name;
            current = current.ContainingNamespace;
        }
    }
}

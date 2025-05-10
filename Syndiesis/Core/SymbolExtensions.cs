using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
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
            is IFieldSymbol { RefKind: RefKind.Ref or RefKind.RefReadOnly }
            or ILocalSymbol { IsRef: true }
            or IParameterSymbol
            {
                RefKind: RefKind.Ref
                    or RefKind.RefReadOnly
                    or RefKind.RefReadOnlyParameter
            }
            or IMethodSymbol { ReturnsByRef: true }
            or IPropertySymbol { ReturnsByRef: true }
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

    public static ImmutableArray<IFieldSymbol> GetFields(this INamedTypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IFieldSymbol>()
            .ToImmutableArray();
    }

    public static ImmutableArray<IPropertySymbol> GetProperties(this INamedTypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToImmutableArray();
    }
}

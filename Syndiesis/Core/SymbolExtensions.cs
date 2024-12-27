using System.Globalization;
using System.Threading;
using System.Xml;
using Microsoft.CodeAnalysis;

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
        if (xml is null)
            return null;
        return CreateXmlDocument(xml);
    }

    private static XmlDocument CreateXmlDocument(string xml)
    {
        var document = new XmlDocument();
        document.Load(xml);
        return document;
    }
}

using System.Xml;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public sealed class XmlDocumentationAnalysisRoot(ISymbol symbol, XmlDocument document)
{
    private readonly ISymbol _symbol = symbol;
    private readonly XmlDocument _document = document;

    // TODO: Retrieve an object that contains all the XML analysis contents

    public static XmlDocumentationAnalysisRoot? CreateForSymbol(ISymbol symbol)
    {
        var document = symbol.GetDocumentationXmlDocument(preferredCulture: null, expandIncludes: true);
        if (document is null)
            return null;

        return new(symbol, document);
    }
}

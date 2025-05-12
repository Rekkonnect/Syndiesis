using Microsoft.CodeAnalysis;
using Syndiesis.Core;
using System.Xml;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed record XmlDocumentationAnalysisRoot(ISymbol Symbol, XmlDocument Document)
{
    // TODO: Retrieve an object that contains all the XML analysis contents

    public static XmlDocumentationAnalysisRoot? CreateForSymbol(ISymbol symbol)
    {
        var document = symbol.GetDocumentationXmlDocument(preferredCulture: null, expandIncludes: true);
        if (document is null)
            return null;

        return new(symbol, document);
    }
}

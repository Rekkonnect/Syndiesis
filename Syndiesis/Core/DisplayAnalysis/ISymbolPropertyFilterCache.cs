using Microsoft.CodeAnalysis;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class ISymbolPropertyFilterCache()
    : PublicApiInterfacePropertyFilterCache<ISymbol>(ISymbolPropertyFilter.Instance)
{
}

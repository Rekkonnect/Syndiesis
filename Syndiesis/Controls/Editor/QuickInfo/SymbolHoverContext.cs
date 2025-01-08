using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed record SymbolHoverContext(
    ISymbol Symbol,
    SemanticModel SemanticModel,
    int TextPosition);

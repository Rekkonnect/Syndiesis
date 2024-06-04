using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public abstract class RoslynColorizer(ISingleTreeCompilationSource compilationSource)
    : DocumentColorizingTransformer
{
    public ISingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    public readonly record struct SymbolTypeKind(SymbolKind SymbolKind, TypeKind TypeKind)
    {
        public static readonly SymbolTypeKind TypeParameter
            = new(SymbolKind.TypeParameter, TypeKind.TypeParameter);

        public static readonly SymbolTypeKind EnumField
            = new SymbolTypeKind() with
            {
                IsEnumField = true
            };

        public bool IsEnumField { get; init; }

        public static implicit operator SymbolTypeKind(SymbolKind kind)
            => new(kind, default);
        public static implicit operator SymbolTypeKind(TypeKind kind)
            => new(SymbolKind.NamedType, kind);
    }
}

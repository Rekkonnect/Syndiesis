using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public abstract class RoslynColorizer(ISingleTreeCompilationSource compilationSource)
    : DocumentColorizingTransformer
{
    public ISingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    public bool Enabled = false;

    protected bool ShouldColorize()
    {
        return Enabled
            && AppSettings.Instance.EnableColorization;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        if (ShouldColorize())
        {
            ColorizeLineEnabled(line);
        }
        else
        {
            ChangeLinePart(line.Offset, line.EndOffset, ResetLine);
        }
    }

    protected void ResetLine(VisualLineElement element)
    {
        element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(Colors.White));
    }

    protected abstract void ColorizeLineEnabled(DocumentLine line);

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

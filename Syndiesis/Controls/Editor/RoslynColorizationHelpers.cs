using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor;

public static class RoslynColorizationHelpers
{
    public static ILazilyUpdatedBrush? BrushForTypeKind(
        RoslynColorizer.ColorizationStyles styles, TypeKind kind)
    {
        return kind switch
        {
            TypeKind.Class => styles.ClassBrush,
            TypeKind.Struct => styles.StructBrush,
            TypeKind.Interface => styles.InterfaceBrush,
            TypeKind.Delegate => styles.DelegateBrush,
            TypeKind.Enum => styles.EnumBrush,
            TypeKind.TypeParameter => styles.TypeParameterBrush,
            TypeKind.Dynamic => styles.KeywordBrush,
            _ => null,
        };
    }
}
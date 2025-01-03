using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public readonly record struct ModifierInfo(
    Accessibility Accessibility,
    MemberModifiers Modifiers
)
{
    public static ModifierInfo GetForSymbol(ISymbol symbol)
    {
        var modifiers
            = CheckModifierOrNone(
                  symbol.HasFileAccessibility(), MemberModifiers.File)
              | CheckModifierOrNone(
                  IsAsync(symbol), MemberModifiers.Async)
              | CheckModifierOrNone(
                  symbol.IsSealed, MemberModifiers.Sealed)
              | CheckModifierOrNone(
                  symbol.IsOverride, MemberModifiers.Override)
              | CheckModifierOrNone(
                  symbol.IsAbstract, MemberModifiers.Abstract)
              | CheckModifierOrNone(
                  symbol.IsVirtual, MemberModifiers.Virtual)
              | CheckModifierOrNone(
                  IsReadOnly(symbol), MemberModifiers.ReadOnly)
              | CheckModifierOrNone(
                  symbol.IsStatic, MemberModifiers.Static)
              | CheckModifierOrNone(
                  IsVolatile(symbol), MemberModifiers.Volatile)
              | CheckModifierOrNone(
                  IsFixedSizeBuffer(symbol), MemberModifiers.FixedSizeBuffer)
              | CheckModifierOrNone(
                  symbol.IsConstant(), MemberModifiers.Const)
              | CheckModifierOrNone(
                  symbol.IsRef(), MemberModifiers.Ref)
            ;
        
        return new(symbol.DeclaredAccessibility, modifiers);
    }

    private static MemberModifiers CheckModifierOrNone(
        bool condition, MemberModifiers modifiers)
    {
        return condition ? modifiers : MemberModifiers.None;
    }

    private static bool IsAsync(ISymbol symbol)
    {
        return symbol is IMethodSymbol { IsAsync: true };
    }

    private static bool IsReadOnly(ISymbol symbol)
    {
        return symbol
            is ITypeSymbol { IsReadOnly: true }
            or IFieldSymbol { IsReadOnly: true }
            or IPropertySymbol { IsReadOnly: true }
            or IMethodSymbol { IsReadOnly: true }
            ;
    }

    private static bool IsVolatile(ISymbol symbol)
    {
        return symbol is IFieldSymbol { IsVolatile: true };
    }

    private static bool IsFixedSizeBuffer(ISymbol symbol)
    {
        return symbol is IFieldSymbol { IsFixedSizeBuffer: true };
    }
}

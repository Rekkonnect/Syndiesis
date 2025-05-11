using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public readonly record struct ModifierInfo(
    Accessibility Accessibility,
    MemberModifiers Modifiers
)
{
    /// <summary>
    /// Gets the modifier combination that was used to declare the symbol that
    /// is being evaluated. Unusable and obvious modifiers like <see langword="sealed"/>
    /// on a <see langword="struct"/> are not returned.
    /// </summary>
    public static ModifierInfo GetForSymbol(ISymbol symbol)
    {
        var modifiers
            = CheckModifierOrNone(
                  symbol.HasFileAccessibility(), MemberModifiers.File)
              | CheckModifierOrNone(
                  IsAsync(symbol), MemberModifiers.Async)
              | CheckModifierOrNone(
                  IsNotInherentlySealed(symbol), MemberModifiers.Sealed)
              | CheckModifierOrNone(
                  symbol.IsOverride, MemberModifiers.Override)
              | CheckModifierOrNone(
                  IsNotInherentlyAbstract(symbol), MemberModifiers.Abstract)
              | CheckModifierOrNone(
                  symbol.IsVirtual, MemberModifiers.Virtual)
              | CheckModifierOrNone(
                  IsNotInherentlyReadOnly(symbol), MemberModifiers.ReadOnly)
              | CheckModifierOrNone(
                  symbol.IsRequired(), MemberModifiers.Required)
              | CheckModifierOrNone(
                  IsNotInherentlyStatic(symbol), MemberModifiers.Static)
              | CheckModifierOrNone(
                  IsVolatile(symbol), MemberModifiers.Volatile)
              | CheckModifierOrNone(
                  IsFixedSizeBuffer(symbol), MemberModifiers.FixedSizeBuffer)
              | CheckModifierOrNone(
                  symbol.IsConstant(), MemberModifiers.Const)
              | CheckModifierOrNone(
                  symbol.IsRef(), MemberModifiers.Ref)
              | CheckModifierOrNone(
                  symbol.IsRefReadOnly(), MemberModifiers.RefReadOnly)
            ;
        
        return new(symbol.DeclaredAccessibility, modifiers);
    }

    private static bool IsNotInherentlySealed(ISymbol symbol)
    {
        return symbol is not ITypeSymbol { IsValueType: true }
            && symbol is { IsSealed: true, IsStatic: false }
            ;
    }
    
    private static bool IsNotInherentlyAbstract(ISymbol symbol)
    {
        return symbol is { IsAbstract: true, IsStatic: false };
    }
    
    private static bool IsNotInherentlyReadOnly(ISymbol symbol)
    {
        return !symbol.IsConstant() && symbol.IsReadOnly();
    }

    private static bool IsNotInherentlyStatic(ISymbol symbol)
    {
        return !symbol.IsConstant() && symbol.IsStatic;
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

    private static bool IsVolatile(ISymbol symbol)
    {
        return symbol is IFieldSymbol { IsVolatile: true };
    }

    private static bool IsFixedSizeBuffer(ISymbol symbol)
    {
        return symbol is IFieldSymbol { IsFixedSizeBuffer: true };
    }
}

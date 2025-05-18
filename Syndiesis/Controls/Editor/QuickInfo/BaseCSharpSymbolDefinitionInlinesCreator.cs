using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Diagnostics.Contracts;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseCSharpSymbolDefinitionInlinesCreator<TSymbol>(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolDefinitionInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected override void AddModifierInlines(
        TSymbol symbol, ComplexGroupedRunInline.Builder inlines)
    {
        var modifierInfo = GetModifierInfo(symbol);
        
        var modifiers = modifierInfo.Modifiers;
        bool isFilePrivate = modifiers.HasFlag(MemberModifiers.File);

        if (isFilePrivate)
        {
            AddKeywordAndSpaceRun("file", inlines);
        }
        else
        {
            var keyword = GetAccessibilityKeyword(modifierInfo.Accessibility);
            if (!string.IsNullOrEmpty(keyword))
            {
                AddKeywordAndSpaceRun(keyword, inlines);
            }
        }
        
        AddTargetModifier(MemberModifiers.Sealed, "sealed");
        AddTargetModifier(MemberModifiers.Override, "override");
        AddTargetModifier(MemberModifiers.Abstract, "abstract");
        AddTargetModifier(MemberModifiers.Virtual, "virtual");
        AddTargetModifier(MemberModifiers.New, "new");
        AddTargetModifier(MemberModifiers.Const, "const");
        AddTargetModifier(MemberModifiers.Static, "static");
        AddTargetModifier(MemberModifiers.Volatile, "volatile");
        AddTargetModifier(MemberModifiers.Required, "required");
        AddTargetModifier(MemberModifiers.FixedSizeBuffer, "fixed");
        
        AddTargetModifier(MemberModifiers.Async, "async");
        AddTargetModifier(MemberModifiers.Extern, "extern");
        
        AddTargetModifier(MemberModifiers.Scoped, "scoped");
        AddTargetModifier(MemberModifiers.ReadOnly, "readonly");
        AddTargetModifier(MemberModifiers.Ref, "ref");
        AddTargetModifier(MemberModifiers.RefReadOnly, "ref readonly");
        AddTargetModifier(MemberModifiers.In, "in");
        AddTargetModifier(MemberModifiers.Out, "out");
        return;

        void AddTargetModifier(MemberModifiers targetFlag, string word)
        {
            AddModifier(inlines, modifiers, targetFlag, word);
        }
    }

    protected virtual ModifierInfo GetModifierInfo(TSymbol symbol)
    {
        return ModifierInfo.GetForSymbol(symbol);
    }

    protected static string GetAccessibilityKeyword(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Private => "private",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => string.Empty,
        };
    }

    protected void AddAccessorInlines(IMethodSymbol? accessor, ComplexGroupedRunInline.Builder inlines)
    {
        if (accessor is null)
            return;

        var accessorAccessibility = accessor.DeclaredAccessibility;
        var associated = accessor.AssociatedSymbol;
        Contract.Assert(associated is not null);
        var associatedAccessibility = associated.DeclaredAccessibility;
        if (accessorAccessibility != associatedAccessibility)
        {
            var accessibilityKeyword = GetAccessibilityKeyword(accessorAccessibility);
            AddModifier(inlines, true, accessibilityKeyword);
        }

        var accessorKeyword = SymbolHelpers.CSharp.KeywordForAccessor(accessor);
        Contract.Assert(accessorKeyword is not null);
        var accessorKeywordRun = SingleKeywordRun(accessorKeyword);
        inlines.Add(accessorKeywordRun);

        inlines.Add(Run("; ", CommonStyles.RawValueBrush));
    }
}

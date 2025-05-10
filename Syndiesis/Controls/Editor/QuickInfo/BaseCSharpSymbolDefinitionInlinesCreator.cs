using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseCSharpSymbolDefinitionInlinesCreator<TSymbol>(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolDefinitionInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected override void AddModifierInlines(
        TSymbol symbol, GroupedRunInlineCollection inlines)
    {
        var modifierInfo = ModifierInfo.GetForSymbol(symbol);
        
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
        AddTargetModifier(MemberModifiers.Ref, "ref");
        AddTargetModifier(MemberModifiers.ReadOnly, "readonly");
        AddTargetModifier(MemberModifiers.Static, "static");
        AddTargetModifier(MemberModifiers.Volatile, "volatile");
        AddTargetModifier(MemberModifiers.FixedSizeBuffer, "fixed");
        
        AddTargetModifier(MemberModifiers.Async, "async");
        AddTargetModifier(MemberModifiers.Const, "const");
        return;

        void AddTargetModifier(MemberModifiers targetFlag, string word)
        {
            AddModifier(inlines, modifiers, targetFlag, word);
        }
    }

    private static string GetAccessibilityKeyword(Accessibility accessibility)
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
}
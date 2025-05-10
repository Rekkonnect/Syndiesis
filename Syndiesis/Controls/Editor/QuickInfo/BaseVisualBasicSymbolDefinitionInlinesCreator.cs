using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseVisualBasicSymbolDefinitionInlinesCreator<TSymbol>(
    VisualBasicSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseSymbolDefinitionInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    protected override void AddModifierInlines(
        TSymbol symbol, GroupedRunInlineCollection inlines)
    {
        var modifierInfo = ModifierInfo.GetForSymbol(symbol);
        
        var keyword = GetAccessibilityKeyword(modifierInfo.Accessibility);
        if (!string.IsNullOrEmpty(keyword))
        {
            AddKeywordAndSpaceRun(keyword, inlines);
        }
        
        var modifiers = modifierInfo.Modifiers;
        AddTargetModifier(MemberModifiers.Sealed, "NotInheritable");
        AddTargetModifier(MemberModifiers.Override, "Overrides");
        AddTargetModifier(MemberModifiers.Abstract, "MustInherit");
        AddTargetModifier(MemberModifiers.Virtual, "Overridable");
        AddTargetModifier(MemberModifiers.New, "Shadows");
        AddTargetModifier(MemberModifiers.Ref, "ByRef");
        AddTargetModifier(MemberModifiers.ReadOnly, "ReadOnly");
        AddTargetModifier(MemberModifiers.Static, "Shared");
        
        AddTargetModifier(MemberModifiers.Async, "Async");
        AddTargetModifier(MemberModifiers.Const, "Const");
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
            Accessibility.Public => "Public",
            Accessibility.Protected => "Protected",
            Accessibility.Friend => "Friend",
            Accessibility.Private => "Private",
            Accessibility.ProtectedOrFriend => "Protected Friend",
            Accessibility.ProtectedAndFriend => "Private Protected",
            _ => string.Empty,
        };
    }
}
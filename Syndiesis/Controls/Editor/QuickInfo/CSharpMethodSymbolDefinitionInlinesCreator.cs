using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IMethodSymbol>(parentContainer)
{
    // Manually implement this to avoid adding duplicate identifiers for the return type
    // whose ref-like semantics are included in the common inlines creator
    protected override void AddModifierInlines(
        IMethodSymbol symbol, ComplexGroupedRunInline.Builder inlines)
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
        AddTargetModifier(MemberModifiers.Static, "static");
        
        AddTargetModifier(MemberModifiers.Async, "async");
        
        AddTargetModifier(MemberModifiers.ReadOnly, "readonly");
        return;

        void AddTargetModifier(MemberModifiers targetFlag, string word)
        {
            AddModifier(inlines, modifiers, targetFlag, word);
        }
    }

    public override void Create(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        AddModifierInlines(method, inlines);
        ParentContainer.RootContainer.Commons
            .CreatorForSymbol(method).Create(method, inlines);
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(IMethodSymbol method)
    {
        return ParentContainer.RootContainer.Commons
            .CreatorForSymbol(method).CreateSymbolInline(method);
    }
}
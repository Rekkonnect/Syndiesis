using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolItemInlinesCreator<TSymbol>(
    BaseSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseInlineCreator, ISymbolItemInlinesCreator
    where TSymbol : class, ISymbol
{
    public static RoslynColorizer.ColorizationStyles ColorizationStyles
        => AppSettings.Instance.ColorizationPreferences.ColorizationStyles!;
    
    public BaseSymbolDefinitionInlinesCreatorContainer ParentContainer { get; } = parentContainer;

    void ISymbolItemInlinesCreator.Create(
        ISymbol symbol, GroupedRunInlineCollection inlines)
    {
        Create((TSymbol)symbol, inlines);  
    }

    public virtual void Create(TSymbol symbol, GroupedRunInlineCollection inlines)
    {
        AddModifierInlines(symbol, inlines);
        var symbolInline = CreateSymbolInline(symbol);
        inlines.Add(symbolInline);
    }

    protected abstract void AddModifierInlines(
        TSymbol symbol, GroupedRunInlineCollection inlines);

    protected abstract GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol);

    protected void AddModifier(
        GroupedRunInlineCollection inlines,
        MemberModifiers memberModifiers,
        MemberModifiers targetFlag,
        string modifierWord)
    {
        var run = ModifierRun(memberModifiers, targetFlag, modifierWord);
        if (run is null)
            return;

        inlines.Add(run);
        inlines.Add(CreateSpaceSeparatorRun());
    }

    protected UIBuilder.Run? ModifierRun(
        MemberModifiers memberModifiers,
        MemberModifiers targetFlag,
        string modifierWord)
    {
        if (!memberModifiers.HasFlag(targetFlag))
            return null;

        return Run(modifierWord, CommonStyles.KeywordBrush);
    }
}
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolQuickInfoInlinesCreator<TSymbol, TParentContainer>(
    TParentContainer parentContainer)
    : BaseInlineCreator, ISymbolItemInlinesCreator
    where TSymbol : class, ISymbol
    where TParentContainer : BaseSymbolInlinesCreatorContainer
{
    public static RoslynColorizer.ColorizationStyles ColorizationStyles
        => AppSettings.Instance.ColorizationPreferences.ColorizationStyles!;

    public TParentContainer ParentContainer { get; } = parentContainer;

    void ISymbolItemInlinesCreator.Create(
        ISymbol symbol, GroupedRunInlineCollection inlines)
    {
        Create((TSymbol)symbol, inlines);
    }

    GroupedRunInline.IBuilder ISymbolItemInlinesCreator.CreateSymbolInline(ISymbol symbol)
    {
        return CreateSymbolInline((TSymbol)symbol);
    }

    void ISymbolItemInlinesCreator.CreateWithHoverContext(
        SymbolHoverContext symbol, GroupedRunInlineCollection inlines)
    {
        CreateWithHoverContext(symbol, inlines);
    }

    public virtual void Create(TSymbol symbol, GroupedRunInlineCollection inlines)
    {
        var symbolInline = CreateSymbolInline(symbol);
        inlines.Add(symbolInline);
    }

    public abstract GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol);

    protected virtual void CreateWithHoverContext(
        SymbolHoverContext context, GroupedRunInlineCollection inlines)
    {
        Create((TSymbol)context.Symbol, inlines);
    }

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

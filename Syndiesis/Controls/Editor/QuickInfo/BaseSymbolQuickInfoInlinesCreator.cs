using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Immutable;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolQuickInfoInlinesCreator<TSymbol, TParentContainer>(
    TParentContainer parentContainer)
    : BaseInlineCreator, ISymbolItemInlinesCreator
    where TSymbol : class, ISymbol
    where TParentContainer : BaseSymbolInlinesCreatorContainer
{
    public TParentContainer ParentContainer { get; } = parentContainer;

    void ISymbolItemInlinesCreator.Create(
        ISymbol symbol, ComplexGroupedRunInline.Builder inlines)
    {
        Create((TSymbol)symbol, inlines);
    }

    GroupedRunInline.IBuilder ISymbolItemInlinesCreator.CreateSymbolInline(ISymbol symbol)
    {
        return CreateSymbolInline((TSymbol)symbol);
    }

    void ISymbolItemInlinesCreator.CreateWithHoverContext(
        SymbolHoverContext context, ComplexGroupedRunInline.Builder inlines)
    {
        CreateWithHoverContext(context, inlines);
    }

    public virtual void Create(TSymbol symbol, ComplexGroupedRunInline.Builder inlines)
    {
        var symbolInline = CreateSymbolInline(symbol);
        inlines.AddChild(symbolInline);
    }

    public abstract GroupedRunInline.IBuilder CreateSymbolInline(TSymbol symbol);

    protected virtual void CreateWithHoverContext(
        SymbolHoverContext context, ComplexGroupedRunInline.Builder inlines)
    {
        Create((TSymbol)context.Symbol, inlines);
    }

    protected void AddModifier(
        ComplexGroupedRunInline.Builder inlines,
        MemberModifiers memberModifiers,
        MemberModifiers targetFlag,
        string modifierWord)
    {
        var run = ModifierRun(memberModifiers, targetFlag, modifierWord);
        AddModifier(inlines, run);
    }

    protected void AddModifier(
        ComplexGroupedRunInline.Builder inlines,
        bool flag,
        string modifierWord)
    {
        var run = ModifierRun(flag, modifierWord);
        AddModifier(inlines, run);
    }

    private static void AddModifier(ComplexGroupedRunInline.Builder inlines, UIBuilder.Run? run)
    {
        if (run is null)
            return;

        inlines.AddChild(run);
        inlines.AddChild(CreateSpaceSeparatorRun());
    }

    protected UIBuilder.Run? ModifierRun(
        MemberModifiers memberModifiers,
        MemberModifiers targetFlag,
        string modifierWord)
    {
        return ModifierRun(memberModifiers.HasFlag(targetFlag), modifierWord);
    }

    protected UIBuilder.Run? ModifierRun(
        bool flag,
        string modifierWord)
    {
        if (!flag)
            return null;

        return Run(modifierWord, CommonStyles.KeywordBrush);
    }
    
    protected ComplexGroupedRunInline.Builder? CreateParameterListInline(
        ImmutableArray<IParameterSymbol> parameters)
    {
        int parameterLength = parameters.Length;
        if (parameterLength is 0)
            return null;

        var inlines = new ComplexGroupedRunInline.Builder();
        var definitions = ParentContainer.RootContainer.Definitions;
        
        for (var i = 0; i < parameterLength; i++)
        {
            var parameter = parameters[i];
            var inner = definitions.CreatorForSymbol(parameter).CreateSymbolInline(parameter);
            inlines.AddChild(inner);

            if (i < parameterLength - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                inlines.AddChild(separator);
            }
        }

        return inlines;
    }
}
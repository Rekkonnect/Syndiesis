using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Collections.Immutable;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseCSharpMemberCommonInlinesCreator<TSymbol>(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCommonMemberCommonInlinesCreator<TSymbol>(parentContainer)
    where TSymbol : class, ISymbol
{
    public new CSharpSymbolCommonInlinesCreatorContainer ParentContainer
        => (CSharpSymbolCommonInlinesCreatorContainer)base.ParentContainer;

    public void AddTypeArgumentInlines(
        ComplexGroupedRunInline.Builder inlines, ImmutableArray<ITypeSymbol> arguments)
    {
        if (arguments.IsDefaultOrEmpty)
        {
            return;
        }

        var openingTag = Run($"<", CommonStyles.RawValueBrush);
        inlines.AddChild(openingTag);
        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];
            var inner = ParentContainer.AliasSimplifiedTypeCreator.CreateSymbolInline(argument);
            inlines.AddChild(inner);

            if (i < arguments.Length - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                inlines.AddChild(separator);
            }
        }

        var closingTag = Run(">", CommonStyles.RawValueBrush);
        inlines.AddChild(closingTag);
    }
}

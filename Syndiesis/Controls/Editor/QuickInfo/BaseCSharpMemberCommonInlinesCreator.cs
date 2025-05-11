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
    
    protected GroupedRunInline.IBuilder? CreateParameterListInline(
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

    protected void AddTypeArgumentInlines(ComplexGroupedRunInline.Builder inlines, ImmutableArray<ITypeSymbol> arguments)
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
            var inner = ParentContainer.TypeCreator.CreateSymbolInline(argument);
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

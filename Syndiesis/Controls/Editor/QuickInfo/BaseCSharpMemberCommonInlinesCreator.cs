using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;

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

        for (var i = 0; i < parameterLength; i++)
        {
            var argument = parameters[i];
            var inner = CreateParameterInline(argument);
            inlines.AddChild(inner);

            if (i < parameterLength - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                inlines.AddChild(separator);
            }
        }

        return inlines;
    }

    private GroupedRunInline.IBuilder CreateParameterInline(IParameterSymbol parameter)
    {
        var type = parameter.Type;
        var typeCreator = ParentContainer.CreatorForSymbol(type);
        Contract.Assert(typeCreator is not null);
        var typeInline = typeCreator.CreateSymbolInline(type);
        var spaceInline = CreateSpaceSeparatorRun();
        var nameInline = SingleRun(parameter.Name, ColorizationStyles.ParameterBrush);

        return new ComplexGroupedRunInline.Builder(
            [new(typeInline), spaceInline, new(nameInline)]);
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

using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using System.Diagnostics.Contracts;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<IMethodSymbol>(parentContainer)
{
    protected override GroupedRunInline.IBuilder CreateSymbolInlineCore(IMethodSymbol method)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        var nameInline = CreateNameAndGenericsInline(method);
        inlines.AddChild(nameInline);
        var openParen = Run("(", CommonStyles.RawValueBrush);
        inlines.AddChild(openParen);
        var parameters = CreateParameterListInline(method);
        inlines.AddNonNullChild(parameters);
        var closeParen = Run(")", CommonStyles.RawValueBrush);
        inlines.AddChild(closeParen);

        return inlines;
    }

    private GroupedRunInline.IBuilder CreateNameAndGenericsInline(IMethodSymbol method)
    {
        var nameRun = SingleRun(method.Name, CommonStyles.MethodBrush);

        if (!method.IsGenericMethod)
        {
            return nameRun;
        }

        var inlines = new ComplexGroupedRunInline.Builder();

        inlines.AddChild(nameRun);
        var openingTag = Run($"<", CommonStyles.RawValueBrush);
        inlines.AddChild(openingTag);
        var parameters = method.TypeParameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var parameterRun = SingleRun(parameter.Name, ColorizationStyles.TypeParameterBrush);
            inlines.AddChild(parameterRun);

            if (i < parameters.Length - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                inlines.AddChild(separator);
            }
        }

        var closingTag = Run(">", CommonStyles.RawValueBrush);
        inlines.AddChild(closingTag);

        return inlines;
    }

    private GroupedRunInline.IBuilder? CreateParameterListInline(IMethodSymbol symbol)
    {
        var parameters = symbol.Parameters;
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
}

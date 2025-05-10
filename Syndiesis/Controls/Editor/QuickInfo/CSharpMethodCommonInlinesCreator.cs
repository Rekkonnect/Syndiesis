using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

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
        var parameters = CreateParameterListInline(method.Parameters);
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
        AddTypeArgumentInlines(inlines, method.TypeArguments);
        return inlines;
    }
}

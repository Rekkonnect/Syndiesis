using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpRangeVariableSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IRangeVariableSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IRangeVariableSymbol range)
    {
        var explanation = Run("(range) ", CommonStyles.NullValueBrush);
        var inline = ParentContainer.RootContainer.Commons.CreatorForSymbol(range).CreateSymbolInline(range);
        return new ComplexGroupedRunInline.Builder([explanation, new(inline)]);
    }
}
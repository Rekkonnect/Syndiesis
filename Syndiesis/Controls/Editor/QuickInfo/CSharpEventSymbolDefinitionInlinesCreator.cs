using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpEventSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IEventSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IEventSymbol @event)
    {
        var type = @event.Type;
        var typeInline = ParentContainer.RootContainer.Commons.CreatorForSymbol(type).CreateSymbolInline(type);
        var space = CreateSpaceSeparatorRun();
        var nameInline = Run(@event.Name, ColorizationStyles.EventBrush);
        return new ComplexGroupedRunInline.Builder([new(typeInline), space, new(nameInline)]);
    }
}

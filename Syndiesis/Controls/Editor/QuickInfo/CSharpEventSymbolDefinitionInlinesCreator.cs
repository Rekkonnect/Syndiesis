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
        var typeInline = ParentContainer.RootContainer.Commons
            .CreatorForSymbol(type).CreateSymbolInline(type);
        var space = CreateSpaceSeparatorRun();
        var nameInline = Run(@event.Name, ColorizationStyles.EventBrush);
        var inlines = new ComplexGroupedRunInline.Builder(
            [new(typeInline), space, new(nameInline)]);
        AppendAccessors(@event, inlines);
        return inlines;
    }

    private void AppendAccessors(IEventSymbol @event, ComplexGroupedRunInline.Builder inlines)
    {
        if (IsSimplifiedDeclaration(@event))
        {
            return;
        }

        var rawBrush = CommonStyles.RawValueBrush;
        var separatorRun = Run(" ", rawBrush);
        inlines.Add(separatorRun);

        var groupedInlines = new ComplexGroupedRunInline.Builder();

        var openRun = Run("{ ", rawBrush);
        groupedInlines.Add(openRun);

        AddAccessorInlines(@event.AddMethod, groupedInlines);
        AddAccessorInlines(@event.RemoveMethod, groupedInlines);

        var closeRun = Run("}", rawBrush);
        groupedInlines.AddChild(closeRun);

        inlines.Add(groupedInlines);
    }

    private static bool IsSimplifiedDeclaration(IEventSymbol @event)
    {
        return @event.AddMethod?.IsImplicitlyDeclared
            ?? @event.RemoveMethod?.IsImplicitlyDeclared
            ?? true;
    }
}

using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpLocalSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<ILocalSymbol>(parentContainer)
{
    public override void Create(ILocalSymbol local, ComplexGroupedRunInline.Builder inlines)
    {
        var inline = CreateSymbolInline(local);
        inlines.AddChild(inline);
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(ILocalSymbol local)
    {
        var builder = new ComplexGroupedRunInline.Builder();
        AddModifierInlines(local, builder);
        
        var type = local.Type;
        var typeCreator = ParentContainer.RootContainer.Commons.CreatorForSymbol(type);
        var typeInline = typeCreator.CreateSymbolInline(type);
        builder.AddChild(typeInline);
        
        builder.AddChild(CreateSpaceSeparatorRun());

        var nameBrush = GetLocalBrush(local);
        var nameInline = SingleRun(local.Name, nameBrush);
        builder.Add(nameInline);
        
        return builder;
    }
}

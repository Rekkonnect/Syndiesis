using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpParameterSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IParameterSymbol>(parentContainer)
{
    public override void Create(IParameterSymbol parameter, ComplexGroupedRunInline.Builder inlines)
    {
        var inline = CreateSymbolInline(parameter);
        inlines.AddChild(inline);
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(IParameterSymbol parameter)
    {
        var builder = new ComplexGroupedRunInline.Builder();
        AddModifierInlines(parameter, builder);
        
        var type = parameter.Type;
        var typeCreator = ParentContainer.RootContainer.Commons.CreatorForSymbol(type);
        var typeInline = typeCreator.CreateSymbolInline(type);
        builder.AddChild(typeInline);
        
        builder.AddChild(CreateSpaceSeparatorRun());
        
        var nameInline = SingleRun(parameter.Name, ColorizationStyles.ParameterBrush);
        builder.AddChild(nameInline);
        
        return builder;
    }
}

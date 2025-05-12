using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonAliasCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSymbolCommonInlinesCreator<IAliasSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IAliasSymbol alias)
    {
        var isGlobalNamespaceAlias = alias is
        {
            Target: INamespaceSymbol { IsGlobalNamespace: true },
            Name: "global",
        };
        if (isGlobalNamespaceAlias)
        {
            return CreateGlobalNamespaceInline();
        }
        
        // TODO: Get the appropriate brush
        var brush = CommonStyles.RawValueBrush;
        var target = alias.Target;
        var targetInline = ParentContainer.CreatorForSymbol(target).CreateSymbolInline(target);

        var builder = new ComplexGroupedRunInline.Builder();

        var inline = SingleRun(alias.Name, brush);
        builder.Add(inline);
        
        return builder;
    }
}
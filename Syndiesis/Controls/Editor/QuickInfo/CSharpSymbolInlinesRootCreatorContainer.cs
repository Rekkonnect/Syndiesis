namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolInlinesRootCreatorContainer
    : BaseSymbolInlinesRootCreatorContainer<
        CSharpSymbolDefinitionInlinesCreatorContainer,
        CSharpSymbolExtraInlinesCreatorContainer,
        CommonSymbolDocsInlinesCreatorContainer,
        CSharpSymbolCommonInlinesCreatorContainer>
{
    protected override CSharpSymbolDefinitionInlinesCreatorContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override CSharpSymbolExtraInlinesCreatorContainer CreateExtraContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override CommonSymbolDocsInlinesCreatorContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override CSharpSymbolCommonInlinesCreatorContainer CreateCommonContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }
}

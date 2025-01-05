namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolInlinesRootCreatorContainer
    : BaseSymbolInlinesRootCreatorContainer<
        VisualBasicSymbolDefinitionInlinesCreatorContainer,
        VisualBasicSymbolExtraInlinesCreatorContainer,
        CommonSymbolDocsInlinesCreatorContainer,
        VisualBasicSymbolCommonInlinesCreatorContainer>
{
    protected override VisualBasicSymbolDefinitionInlinesCreatorContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override VisualBasicSymbolExtraInlinesCreatorContainer CreateExtraContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override CommonSymbolDocsInlinesCreatorContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override VisualBasicSymbolCommonInlinesCreatorContainer CreateCommonContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }
}

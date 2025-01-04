namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolInlinesRootCreatorContainer<
    TDefinitionContainer,
    TExtraContainer,
    TDocsContainer,
    TCommonContainer>
    : ISymbolInlinesRootCreatorContainer

    where TDefinitionContainer : BaseSymbolDefinitionInlinesCreatorContainer
    where TExtraContainer : BaseSymbolExtraInlinesCreatorContainer
    where TDocsContainer : BaseSymbolDocsInlinesCreatorContainer
    where TCommonContainer : BaseSymbolCommonInlinesCreatorContainer
{
    public readonly TDefinitionContainer Definitions;
    public readonly TExtraContainer Extras;
    public readonly TDocsContainer Docs;
    public readonly TCommonContainer Commons;

    protected BaseSymbolInlinesRootCreatorContainer()
    {
        Definitions = CreateDefinitionContainer(this);
        Extras = CreateExtraContainer(this);
        Docs = CreateDocsContainer(this);
        Commons = CreateCommonContainer(this);
    }

    protected abstract TDefinitionContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TExtraContainer CreateExtraContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TDocsContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TCommonContainer CreateCommonContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
}

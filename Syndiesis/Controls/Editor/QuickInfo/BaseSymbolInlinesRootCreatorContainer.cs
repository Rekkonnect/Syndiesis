namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolInlinesRootCreatorContainer<
    TDefinitionContainer,
    TExtrasContainer,
    TDocsContainer,
    TCommonsContainer>
    : ISymbolInlinesRootCreatorContainer

    where TDefinitionContainer : BaseSymbolDefinitionInlinesCreatorContainer
{
    public readonly TDefinitionContainer Definitions;
    public readonly TExtrasContainer Extras;
    public readonly TDocsContainer Docs;
    public readonly TCommonsContainer Commons;

    protected BaseSymbolInlinesRootCreatorContainer()
    {
        Definitions = CreateDefinitionContainer(this);
        Extras = CreateExtrasContainer(this);
        Docs = CreateDocsContainer(this);
        Commons = CreateCommonsContainer(this);
    }

    protected abstract TDefinitionContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TExtrasContainer CreateExtrasContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TDocsContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
    protected abstract TCommonsContainer CreateCommonsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer);
}

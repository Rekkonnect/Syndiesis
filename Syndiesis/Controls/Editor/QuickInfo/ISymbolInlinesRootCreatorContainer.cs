namespace Syndiesis.Controls.Editor.QuickInfo;

// Marker interface to avoid the generic explosion from the real base type
public interface ISymbolInlinesRootCreatorContainer
{
    public abstract BaseSymbolDefinitionInlinesCreatorContainer Definitions { get; }
    public abstract BaseSymbolExtraInlinesCreatorContainer Extras { get; }
    public abstract BaseSymbolDocsInlinesCreatorContainer Docs { get; }
    public abstract BaseSymbolCommonInlinesCreatorContainer Commons { get; }
}

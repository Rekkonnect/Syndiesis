using Syndiesis.Controls.AnalysisVisualization;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract class BaseNodeLineCreator(NodeLineCreationOptions options)
{
    protected readonly NodeLineCreationOptions Options = options;

    public abstract AnalysisTreeListNode? CreateRootViewNode(
        object? value, DisplayValueSource valueSource = default);
}

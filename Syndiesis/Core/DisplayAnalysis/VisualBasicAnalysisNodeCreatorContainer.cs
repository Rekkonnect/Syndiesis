namespace Syndiesis.Core.DisplayAnalysis;

public sealed class VisualBasicAnalysisNodeCreatorContainer : BaseAnalysisNodeCreatorContainer
{
    protected override BaseSyntaxAnalysisNodeCreator CreateSyntaxCreator()
    {
        return new VisualBasicSyntaxAnalysisNodeCreator(this);
    }
}

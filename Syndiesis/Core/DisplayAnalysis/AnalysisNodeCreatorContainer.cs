namespace Syndiesis.Core.DisplayAnalysis;

public sealed class AnalysisNodeCreatorContainer
{
    public readonly SyntaxAnalysisNodeCreator SyntaxCreator;
    public readonly SymbolAnalysisNodeCreator SymbolCreator;
    public readonly OperationsAnalysisNodeCreator OperationCreator;
    public readonly SemanticModelAnalysisNodeCreator SemanticCreator;

    public AnalysisNodeCreatorContainer()
    {
        SyntaxCreator = new(this);
        SymbolCreator = new(this);
        OperationCreator = new(this);
        SemanticCreator = new(this);
    }
}

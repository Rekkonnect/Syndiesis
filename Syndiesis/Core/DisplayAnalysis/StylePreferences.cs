using Syndiesis.Controls;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class StylePreferences
{
    public BaseAnalysisNodeCreator.NodeCommonStyles? CommonStyles;
    public BaseSyntaxAnalysisNodeCreator.SyntaxStyles? SyntaxStyles;
    public SymbolAnalysisNodeCreator.SymbolStyles? SymbolStyles;
    public OperationsAnalysisNodeCreator.OperationStyles? OperationStyles;
    public SemanticModelAnalysisNodeCreator.SemanticModelStyles? SemanticModelStyles;
    public AttributesAnalysisNodeCreator.AttributeStyles? AttributeStyles;

    public StylePreferences()
    {
        Dispatcher.UIThread.ExecuteOrDispatch(Initialize);

        void Initialize()
        {
            CommonStyles = new();
            SyntaxStyles = new();
            SymbolStyles = new();
            OperationStyles = new();
            SemanticModelStyles = new();
            AttributeStyles = new();
        }
    }
}

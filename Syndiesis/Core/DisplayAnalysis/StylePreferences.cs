using Avalonia.Threading;
using Syndiesis.Controls;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class StylePreferences
{
    public BaseAnalysisNodeCreator.NodeCommonStyles? CommonStyles;
    public SyntaxAnalysisNodeCreator.SyntaxStyles? SyntaxStyles;
    public SymbolAnalysisNodeCreator.SymbolStyles? SymbolStyles;
    public OperationsAnalysisNodeCreator.OperationStyles? OperationStyles;
    public SemanticModelAnalysisNodeCreator.SemanticModelStyles? SemanticModelStyles;

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
        }
    }
}

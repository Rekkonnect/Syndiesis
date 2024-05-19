using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Views.DesignerPreviews;

public partial class AnalysisTreeListViewExampleFromSource : AnalysisTreeListView
{
    public AnalysisTreeListViewExampleFromSource()
    {
        InitializeComponent();
        TriggerCodeExample();
    }

    private void TriggerCodeExample()
    {
        const string source = """
            public void AddIndexed<TGeometry, TVertex>(TGeometry geometry)
                where TGeometry : unmanaged, IGeometry<TGeometry, TVertex>, IIndexedGeometry
                where TVertex : unmanaged, IVertex
            {
                if (currentIndices + 1 >= maxBufferSize || currentVertices + 1 >= maxBufferSize)
                {
                    Flush();
                }
            }
            """;

        var options = new AnalysisNodeCreationOptions();
        var creator = new SyntaxAnalysisNodeCreator(options);

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();

        var nodeRoot = creator.CreateRootNode(compilationUnitRoot);
        RootNode = nodeRoot;
    }
}

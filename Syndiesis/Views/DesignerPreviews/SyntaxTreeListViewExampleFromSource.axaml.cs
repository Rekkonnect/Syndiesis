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
        var container = new AnalysisNodeCreatorContainer(options);
        var creator = container.SyntaxCreator;

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var nodeRoot = creator.CreateRootTree(syntaxTree);
        RootNode = nodeRoot.Build();
    }
}

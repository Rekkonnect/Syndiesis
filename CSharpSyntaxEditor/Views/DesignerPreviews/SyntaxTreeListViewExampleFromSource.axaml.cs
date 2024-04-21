using Avalonia.Interactivity;
using CSharpSyntaxEditor.Controls;
using CSharpSyntaxEditor.Controls.SyntaxVisualization.Creation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpSyntaxEditor.Views.DesignerPreviews;

public partial class SyntaxTreeListViewExampleFromSource : SyntaxTreeListView
{
    public SyntaxTreeListViewExampleFromSource()
    {
        InitializeComponent();
        TriggerCodeExample();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
    }

    private void TriggerCodeExample()
    {
        const string source = """
            public void AddIndexed<TGeometry, TVertex>(TGeometry geometry)
                where TGeometry : unmanaged, IGeometry<TGeometry, TVertex>, IIndexedGeometry
                where TVertex : unmanaged, IVertex
            {
                if (currentIndices + 1 >= maxBufferSize || currentVertexs + 1 >= maxBufferSize)
                {
                    Flush();
                }
            }
            """;

        var options = new NodeLineCreationOptions();
        var creator = new NodeLineCreator(options);

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();

        // debug
        var globalStatement = compilationUnitRoot.Members.First() as GlobalStatementSyntax;
        var localFunction = globalStatement.Statement as LocalFunctionStatementSyntax;

        var nodeRoot = creator.CreateRootNode(compilationUnitRoot);
        RootNode = nodeRoot;
    }
}

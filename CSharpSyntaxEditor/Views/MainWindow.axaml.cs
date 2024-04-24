using Avalonia.Controls;
using Avalonia.Interactivity;
using CSharpSyntaxEditor.ViewModels;
using System;

namespace CSharpSyntaxEditor.Views;

public partial class MainWindow : Window
{
    private readonly AnalysisPipelineHandler _analysisPipelineHandler = new();

    private readonly MainWindowViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();

        codeEditor.CodeChanged += TriggerPipeline;
        syntaxTreeView.RegisterAnalysisPipelineHandler(_analysisPipelineHandler);
    }

    private void TriggerPipeline()
    {
        var currentSource = _viewModel.Editor.FullString();
        _analysisPipelineHandler.InitiateAnalysis(currentSource);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Reset();
    }

    private void Reset()
    {
        const string defaultCode = """
            #define SYNDIESIS

            using System;

            namespace Example;

            public class Program
            {
                public static void Main(string[] args)
                {
                    // using conditional compilation symbols is fun
                    const string greetings =
            #if SYNDIESIS
                        "Hello Syndiesis!"
            #else
                        "Hello World!"
            #endif
                        ;
                    Console.WriteLine(greetings);
                }
            }

            """;

        codeEditor.Editor = _viewModel.Editor;
        var previousDelay = _analysisPipelineHandler.UserInputDelay;
        _analysisPipelineHandler.UserInputDelay = TimeSpan.Zero;
        codeEditor.SetSource(defaultCode);
        _analysisPipelineHandler.UserInputDelay = previousDelay;
    }
}

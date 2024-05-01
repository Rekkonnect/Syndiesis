using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls;
using Syndiesis.Utilities.Specific;
using Syndiesis.ViewModels;
using System;
using System.Threading.Tasks;

namespace Syndiesis.Views;

public partial class MainView : UserControl
{
    public readonly AnalysisPipelineHandler AnalysisPipelineHandler = new();

    public readonly MainWindowViewModel ViewModel = new();

    public event Action? SettingsRequested;

    public MainView()
    {
        InitializeComponent();
        InitializeView();
        InitializeEvents();

        Focusable = true;
    }

    private void InitializeView()
    {
        const string initializingSource = """
            using System;

            namespace Example;

            Console.WriteLine("Initializing application...");

            """;

        codeEditor.SetSource(initializingSource);
        codeEditor.CursorPosition = new(4, 48);
    }

    private void InitializeEvents()
    {
        codeEditor.CodeChanged += TriggerPipeline;
        codeEditor.CursorPositionChanged += HandleCursorPositionChanged;
        syntaxTreeView.listView.HoveredNode += HandleHoveredNode;
        syntaxTreeView.NewRootNodeLoaded += HandleNewRootNodeLoaded;

        syntaxTreeView.RegisterAnalysisPipelineHandler(AnalysisPipelineHandler);

        InitializeButtonEvents();
    }

    private void InitializeButtonEvents()
    {
        resetCodeButton.Click += HandleResetClick;
        pasteOverButton.Click += HandlePasteOverClick;
        settingsButton.Click += HandleSettingsClick;
        collapseAllButton.Click += CollapseAllClick;
        expandAllButton.Click += ExpandAllClick;
    }

    private void ExpandAllClick(object? sender, RoutedEventArgs e)
    {
        syntaxTreeView.listView.RootNode.SetExpansionWithoutAnimationRecursively(true);
    }

    private void CollapseAllClick(object? sender, RoutedEventArgs e)
    {
        syntaxTreeView.listView.RootNode.SetExpansionWithoutAnimationRecursively(false);
    }

    private void HandleSettingsClick(object? sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke();
    }

    private void HandlePasteOverClick(object? sender, RoutedEventArgs e)
    {
        _ = HandlePasteClick();
    }

    private async Task HandlePasteClick()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        var pasteText = await clipboard.GetTextAsync();
        if (pasteText is null)
            return;

        SetSource(pasteText);
    }

    private void HandleResetClick(object? sender, RoutedEventArgs e)
    {
        Reset();
    }

    private void HandleNewRootNodeLoaded()
    {
        // trigger showing the current position of the cursor
        var position = codeEditor.CursorPosition;
        ShowCurrentCursorPosition(position);
    }

    private void HandleCursorPositionChanged(LinePosition position)
    {
        ShowCurrentCursorPosition(position);
    }

    private void ShowCurrentCursorPosition(LinePosition position)
    {
        var index = ViewModel.Editor.GetIndex(position);
        syntaxTreeView.listView.HighlightPosition(index);
    }

    private void HandleHoveredNode(SyntaxTreeListNode? obj)
    {
        codeEditor.ShowHoveredSyntaxNode(obj);
    }

    private void TriggerPipeline()
    {
        var currentSource = ViewModel.Editor.FullString();
        AnalysisPipelineHandler.InitiateAnalysis(currentSource);
    }

    public void ApplyCurrentSettings()
    {
        var settings = AppSettings.Instance;
        AnalysisPipelineHandler.CreationOptions = settings.CreationOptions;
        AnalysisPipelineHandler.UserInputDelay = settings.UserInputDelay;
        expandAllButton.IsVisible = settings.EnableExpandingAllNodes;
        ForceRedoAnalysis();
    }

    public void Reset()
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

        SetSource(defaultCode);
    }

    private void SetSource(string source)
    {
        var analysisPipelineHandler = AnalysisPipelineHandler;
        var viewModel = ViewModel;

        codeEditor.Editor = viewModel.Editor;
        var previousDelay = analysisPipelineHandler.UserInputDelay;
        analysisPipelineHandler.UserInputDelay = TimeSpan.Zero;
        codeEditor.SetSource(source);
        analysisPipelineHandler.UserInputDelay = previousDelay;
    }

    public void ForceRedoAnalysis()
    {
        var analysisPipelineHandler = AnalysisPipelineHandler;

        var previousDelay = analysisPipelineHandler.UserInputDelay;
        analysisPipelineHandler.UserInputDelay = TimeSpan.Zero;
        TriggerPipeline();
        analysisPipelineHandler.UserInputDelay = previousDelay;
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        codeEditor.Focus();
    }
}

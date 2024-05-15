using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using Syndiesis.Controls;
using Syndiesis.Core;
using Syndiesis.Utilities;
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
        ApplyCurrentSettingsWithoutAnalysis();

        Focusable = true;
    }

    private void InitializeView()
    {
        const string initializingSource = """
            using System;

            namespace Example;

            Console.WriteLine("Initializing application...");

            """;

        LoggerExtensionsEx.LogMethodInvocation(nameof(InitializeView));

        codeEditor.Editor = ViewModel.Editor;
        codeEditor.SetSource(initializingSource);
        codeEditor.CursorPosition = new(4, 48);
        codeEditor.AssociatedTreeView = syntaxTreeView.listView;
    }

    private void InitializeEvents()
    {
        codeEditor.CodeChanged += TriggerPipeline;
        codeEditor.CursorMoved += HandleCursorPositionChanged;
        syntaxTreeView.listView.HoveredNode += HandleHoveredNode;
        syntaxTreeView.listView.RequestedPlaceCursorAtNode += HandleRequestedPlaceCursorAtNode;
        syntaxTreeView.listView.RequestedSelectTextAtNode += HandleRequestedSelectTextAtNode;
        syntaxTreeView.NewRootNodeLoaded += HandleNewRootNodeLoaded;

        syntaxTreeView.RegisterAnalysisPipelineHandler(AnalysisPipelineHandler);

        InitializeButtonEvents();
        InitializeComboBoxEvents();
    }

    private void InitializeButtonEvents()
    {
        resetCodeButton.Click += HandleResetClick;
        pasteOverButton.Click += HandlePasteOverClick;
        settingsButton.Click += HandleSettingsClick;
        collapseAllButton.Click += CollapseAllClick;
        expandAllButton.Click += ExpandAllClick;
    }

    private void InitializeComboBoxEvents()
    {
        viewModeComboBox.SelectedItem = SyntaxTree;
        viewModeComboBox.SelectionChanged += HandleViewModeClick;
    }

    private void ExpandAllClick(object? sender, RoutedEventArgs e)
    {
        ExpandAllNodes();
    }

    private void ExpandAllNodes()
    {
        Log.Information("Began expanding all nodes");
        var profiling = new SimpleProfiling();
        using (profiling.BeginProcess())
        {
            syntaxTreeView.listView.RootNode.SetExpansionWithoutAnimationRecursively(true);
        }
        var results = profiling.SnapshotResults!;
        Log.Information(
            $"Expanding all nodes took {results.Time.TotalMilliseconds:N2}ms " +
            $"and reserved {results.Memory:N0} bytes");
    }

    private void CollapseAllClick(object? sender, RoutedEventArgs e)
    {
        syntaxTreeView.listView.CollapseAll();
    }

    private void HandleViewModeClick(object? sender, RoutedEventArgs e)
    {
        switch (((ComboBoxItem)viewModeComboBox.SelectedItem!).Name)
        {
            case "SyntaxTree":
                break;

            case "IL":
                break;

            case "CS":
                break;
        }
    }

    private void HandleSettingsClick(object? sender, RoutedEventArgs e)
    {
        RequestSettings();
    }

    private void RequestSettings()
    {
        SettingsRequested?.Invoke();
    }

    private void HandlePasteOverClick(object? sender, RoutedEventArgs e)
    {
        _ = HandlePasteClick();
    }

    private async Task HandlePasteClick()
    {
        var pasteText = await this.GetClipboardTextAsync();
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
        var index = ViewModel.Editor.MultilineEditor.GetIndex(position);
        syntaxTreeView.listView.HighlightPosition(index);
    }

    private void HandleHoveredNode(SyntaxTreeListNode? obj)
    {
        codeEditor.ShowHoveredSyntaxNode(obj);
    }

    private void HandleRequestedSelectTextAtNode(SyntaxTreeListNode node)
    {
        codeEditor.SelectTextOfNode(node);
    }

    private void HandleRequestedPlaceCursorAtNode(SyntaxTreeListNode node)
    {
        codeEditor.PlaceCursorAtNodeStart(node);
    }

    private void TriggerPipeline()
    {
        var currentSource = ViewModel.Editor.MultilineEditor.FullString();
        AnalysisPipelineHandler.InitiateAnalysis(currentSource);
    }

    public void ApplyCurrentSettings()
    {
        ApplyCurrentSettingsWithoutAnalysis();
        ViewModel.Editor.IndentationOptions = AppSettings.Instance.IndentationOptions;
        ForceRedoAnalysis();
    }

    private void ApplyCurrentSettingsWithoutAnalysis()
    {
        var settings = AppSettings.Instance;
        AnalysisPipelineHandler.AnalysisExecution.NodeLineOptions = settings.NodeLineOptions;
        AnalysisPipelineHandler.UserInputDelay = settings.UserInputDelay;
        expandAllButton.IsVisible = settings.EnableExpandingAllNodes;
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

        LoggerExtensionsEx.LogMethodInvocation(nameof(Reset));

        SetSource(defaultCode);
    }

    private void SetSource(string source)
    {
        var analysisPipelineHandler = AnalysisPipelineHandler;

        analysisPipelineHandler.IgnoreInputDelayOnce();
        codeEditor.SetSource(source);
    }

    public void ForceRedoAnalysis()
    {
        var analysisPipelineHandler = AnalysisPipelineHandler;

        analysisPipelineHandler.IgnoreInputDelayOnce();
        TriggerPipeline();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        codeEditor.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;

        bool hasControl = modifiers.HasFlag(KeyModifiers.Control);

        switch (e.Key)
        {
            case Key.S:
                if (hasControl)
                {
                    RequestSettings();
                    e.Handled = true;
                }
                break;

            case Key.R:
                if (hasControl)
                {
                    Reset();
                    e.Handled = true;
                }
                break;
        }

        base.OnKeyDown(e);
    }
}

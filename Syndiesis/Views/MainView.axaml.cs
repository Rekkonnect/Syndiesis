using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using Syndiesis.Controls;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Tabs;
using Syndiesis.Core;
using Syndiesis.Utilities;
using Syndiesis.ViewModels;
using System;
using System.Diagnostics;
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
    }

    private void InitializeView()
    {
        const string initializingSource = """
            using System;

            namespace Example;

            Console.WriteLine("Initializing application...");

            """;

        LoggerExtensionsEx.LogMethodInvocation(nameof(InitializeView));

        ViewModel.CompilationSource.SetSource(initializingSource, default);

        codeEditor.Editor = ViewModel.Editor;
        codeEditor.SetSource(initializingSource);
        codeEditor.CursorPosition = new(4, 48);
        codeEditor.AssociatedTreeView = syntaxTreeView.listView;

        analysisTreeViewTabs.Envelopes =
        [
            Envelope("Syntax", AnalysisNodeKind.Syntax),
            Envelope("Operations", AnalysisNodeKind.Operation),
            Envelope("Symbols", AnalysisNodeKind.Symbol),
        ];

        analysisTreeViewTabs.TabSelected += HandleSelectedAnalysisTab;

        analysisTreeViewTabs.SelectIndex(0);

        static TabEnvelope Envelope(string text, AnalysisNodeKind analysisKind)
        {
            return new()
            {
                Text = text,
                MinWidth = 100,
                TagValue = analysisKind,
            };
        }
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
    }

    private void InitializeButtonEvents()
    {
        resetCodeButton.Click += HandleResetClick;
        pasteOverButton.Click += HandlePasteOverClick;
        settingsButton.Click += HandleSettingsClick;
        collapseAllButton.Click += CollapseAllClick;
        expandAllButton.Click += ExpandAllClick;
        githubButton.Click += GitHubClick;
    }

    private void HandleSelectedAnalysisTab(TabEnvelope tab)
    {
        var analysisKind = (AnalysisNodeKind)tab.TagValue!;
        var analysisFactory = new AnalysisExecutionFactory(ViewModel.CompilationSource);
        var analysisExecution = analysisFactory.CreateAnalysisExecution(analysisKind);
        AnalysisPipelineHandler.AnalysisExecution = analysisExecution;
        _ = AnalysisPipelineHandler.ForceAnalysis();
    }

    private void GitHubClick(object? sender, RoutedEventArgs e)
    {
        const string githubLink = "https://github.com/Rekkonnect/Syndiesis";
        ProcessUtilities.OpenUrl(githubLink);
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

    private void HandleHoveredNode(AnalysisTreeListNode? obj)
    {
        codeEditor.ShowHoveredSyntaxNode(obj);
    }

    private void HandleRequestedSelectTextAtNode(AnalysisTreeListNode node)
    {
        codeEditor.SelectTextOfNode(node);
    }

    private void HandleRequestedPlaceCursorAtNode(AnalysisTreeListNode node)
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
        var analysisExecution = AnalysisPipelineHandler.AnalysisExecution;
        if (analysisExecution is not null)
        {
            analysisExecution.NodeLineOptions = settings.NodeLineOptions;
        }

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
        if (!e.Handled)
        {
            codeEditor.Focus();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();

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

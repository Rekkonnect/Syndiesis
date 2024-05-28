using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using Syndiesis.Controls;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Tabs;
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
        InitializeAnalysisView();
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

        codeEditor.textEditor.Document = ViewModel.Document;
        codeEditor.SetSource(initializingSource);
        codeEditor.CaretPosition = new(4, 48);
        codeEditor.AssociatedTreeView = syntaxTreeView.listView;

        analysisTreeViewTabs.Envelopes =
        [
            Envelope("Syntax", AnalysisNodeKind.Syntax),
            Envelope("Symbols", AnalysisNodeKind.Symbol),
            Envelope("Operations", AnalysisNodeKind.Operation),
        ];

        analysisTreeViewTabs.TabSelected += HandleSelectedAnalysisTab;

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
        codeEditor.CodeChanged += HandleCodeChanged;
        codeEditor.CaretMoved += HandleCursorPositionChanged;
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
        githubButton.Click += GitHubClick;
    }

    private void InitializeAnalysisView()
    {
        analysisTreeViewTabs.SelectIndex(0);
    }

    private void HandleSelectedAnalysisTab(TabEnvelope tab)
    {
        var analysisKind = (AnalysisNodeKind)tab.TagValue!;
        var analysisFactory = new AnalysisExecutionFactory(ViewModel.CompilationSource);
        var analysisExecution = analysisFactory.CreateAnalysisExecution(analysisKind);
        AnalysisPipelineHandler.AnalysisExecution = analysisExecution;
        AnalysisPipelineHandler.IgnoreInputDelayOnce();
        if (IsLoaded)
        {
            Task.Run(ForceAnalysisResetView);
        }
    }

    private async Task ForceAnalysisResetView()
    {
        await Task.Run(AnalysisPipelineHandler.ForceAnalysis);
    }

    private void GitHubClick(object? sender, RoutedEventArgs e)
    {
        const string githubLink = "https://github.com/Rekkonnect/Syndiesis";
        ProcessUtilities.OpenUrl(githubLink);
    }

    private void CollapseAllClick(object? sender, RoutedEventArgs e)
    {
        syntaxTreeView.listView.ResetToInitialRootView();
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
        var pasteText = await Task.Run(this.GetClipboardTextAsync);
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
        RefreshCaretPosition();
    }

    private void HandleCursorPositionChanged(object? sender, EventArgs e)
    {
        RefreshCaretPosition();
    }

    private void RefreshCaretPosition()
    {
        var position = codeEditor.CaretPosition;
        ShowCurrentCursorPosition(position);
    }

    private void ShowCurrentCursorPosition(TextViewPosition position)
    {
        int index = codeEditor.textEditor.TextArea.Document.GetOffset(position.Location);
        Task.Run(() => syntaxTreeView.listView.EnsureHighlightedPositionRecurring(index));
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

    private void HandleCodeChanged(object? sender, EventArgs e)
    {
        TriggerPipeline();
    }

    private void TriggerPipeline()
    {
        var currentSource = ViewModel.Document.Text;
        AnalysisPipelineHandler.InitiateAnalysis(currentSource);
    }

    public void ApplyCurrentSettings()
    {
        ApplyCurrentSettingsWithoutAnalysis();
        codeEditor.ApplyIndentationOptions(AppSettings.Instance.IndentationOptions);
        ForceRedoAnalysis();
    }

    private void ApplyCurrentSettingsWithoutAnalysis()
    {
        var settings = AppSettings.Instance;
        var analysisExecution = AnalysisPipelineHandler.AnalysisExecution;
        if (analysisExecution is not null)
        {
            analysisExecution.CreationOptions = settings.NodeLineOptions;
        }

        AnalysisPipelineHandler.UserInputDelay = settings.UserInputDelay;
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

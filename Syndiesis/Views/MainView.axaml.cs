using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Toast;
using Syndiesis.Core;
using Syndiesis.Utilities;
using Syndiesis.ViewModels;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Syndiesis.Views;

public partial class MainView : UserControl
{
    private QuickInfoHandler _quickInfoHandler;

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

    [MemberNotNull(nameof(_quickInfoHandler))]
    private void InitializeView()
    {
        _quickInfoHandler = new(quickInfoDisplayPopup);
        _quickInfoHandler.RegisterMovementHandling(this);
        _quickInfoHandler.PrepareShowing += PrepareQuickInfoShowing;

        LoggerExtensionsEx.LogMethodInvocation(nameof(InitializeView));

        const string initializingSource = """
            using System;

            namespace Example;

            Console.WriteLine("Initializing application...");

            """;

        ViewModel.CompilationSource.SetSource(initializingSource, default);

        codeEditor.Document = ViewModel.Document;
        codeEditor.SetSource(initializingSource);
        codeEditor.CaretPosition = new(4, 48);
        codeEditor.AssociatedTreeView = coverableView.ListView;

        codeEditor.CompilationSource = ViewModel.HybridCompilationSource;

        analysisViewTabs.TabSelected += HandleSelectedAnalysisTab;
    }

    private void PrepareQuickInfoShowing(QuickInfoHandler.PrepareShowingEventArgs e)
    {
        var pointerArgs = e.LastPointerArgs;
        if (pointerArgs is null)
            return;

        var viewPosition = pointerArgs.GetPosition(this);
        var editorPosition = pointerArgs.GetPosition(codeEditor.textEditor);
        var diagnostics = GetCurrentHoveredDiagnostics(editorPosition);

        if (diagnostics.IsEmpty)
        {
            e.CancelShowing = true;
            return;
        }

        quickInfoDisplayPopup.SetDiagnostics(diagnostics);
        quickInfoDisplayPopup.SetPointerOrigin(viewPosition);
    }

    private void SetBoundedMargin(Control control, Point origin)
    {
        var bounds = Bounds;
        var controlSize = control.Bounds.Size;

        control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        control.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        control.Margin = new(0, 0, 0, 0);

        control.Measure(bounds.Size);

        if (bounds.Width >= controlSize.Width)
        {
            if (origin.X + controlSize.Width >= bounds.Width)
            {
                control.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                var rightMargin = bounds.Right - origin.X;
                if (rightMargin < controlSize.Width)
                {
                    rightMargin = 0;
                }

                control.Margin = control.Margin
                    .WithLeft(0)
                    .WithRight(rightMargin)
                    ;
            }
        }

        if (bounds.Height >= controlSize.Height)
        {
            if (origin.Y + controlSize.Height >= bounds.Height)
            {
                control.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

                var bottomMargin = bounds.Bottom - origin.Y;
                if (bottomMargin < controlSize.Height)
                {
                    bottomMargin = 0;
                }

                control.Margin = control.Margin
                    .WithTop(0)
                    .WithBottom(bottomMargin)
                    ;
            }
        }
    }

    private ImmutableArray<Diagnostic> GetCurrentHoveredDiagnostics(Point point)
    {
        var documentPosition = codeEditor.textEditor.GetPositionFromPoint(point) ?? default;
        if (documentPosition == default)
            return [];

        var linePosition = new LinePosition(
            documentPosition.Line - 1,
            documentPosition.Column - 1);
        var diagnostics = codeEditor.CompilationSource?.CurrentSource.Diagnostics
            .DiagnosticsAtPosition(linePosition)
            .ToImmutableArray()
            ?? [];
        return diagnostics;
    }

    private void InitializeEvents()
    {
        codeEditor.TextChanged += HandleCodeChanged;
        codeEditor.CaretMoved += HandleCaretPositionChanged;
        codeEditor.SelectionChanged += HandleSelectionChanged;
        coverableView.ListView.HoveredNode += HandleHoveredNode;
        coverableView.ListView.RequestedPlaceCursorAtNode += HandleRequestedPlaceCursorAtNode;
        coverableView.ListView.RequestedSelectTextAtNode += HandleRequestedSelectTextAtNode;
        coverableView.ListView.NewRootLoaded += HandleNewRootNodeLoaded;
        coverableView.ListView.CaretHoveredNodeSet += HandleCaretHoveredNode;

        coverableView.NodeDetailsView.HoveredNode += HandleDetailsViewHoveredNode;
        coverableView.NodeDetailsView.CaretHoveredNodeSet += HandleDetailsViewCaretHoveredNode;

        languageVersionDropDown.LanguageVersionChanged += SetLanguageVersion;

        AnalysisPipelineHandler.AnalysisCompleted += OnAnalysisCompleted;

        codeEditor.RegisterAnalysisPipelineHandler(AnalysisPipelineHandler);
        coverableView.RegisterAnalysisPipelineHandler(AnalysisPipelineHandler);

        InitializeButtonEvents();
    }

    private AnalysisTreeListNode? _caretHoveredNode = null;

    private void HandleDetailsViewCaretHoveredNode(AnalysisTreeListNode? node)
    {
        HandleCaretHoveredNode(node);
        HandleHoveredNode(node);
    }

    private void HandleCaretHoveredNode(AnalysisTreeListNode? node)
    {
        _caretHoveredNode = node;
    }

    private void OnAnalysisCompleted(AnalysisResult result)
    {
        _pendingDocumentAnalysis = false;

        void UpdateUI()
        {
            var version = ViewModel.HybridCompilationSource.CurrentSource.LanguageVersion;
            languageVersionDropDown.DisplayVersion(version);
            RefreshCaretPosition();

            if (analysisViewTabs.AnalysisViewKind is AnalysisViewKind.Tree)
            {
                switch (result)
                {
                    case NodeRootAnalysisResult result:
                        coverableView.ListView.RootNode = result.NodeRoot.Build()!;
                        coverableView.ListView.TargetAnalysisNodeKind = result.TargetAnalysisNodeKind;
                        break;
                }
            }
        }

        Dispatcher.UIThread.InvokeAsync(UpdateUI);
    }

    private void InitializeButtonEvents()
    {
        resetCodeButton.Click += HandleResetClick;
        pasteOverButton.Click += HandlePasteOverClick;
        settingsButton.Click += HandleSettingsClick;
        collapseAllButton.Click += CollapseAllClick;
        githubButton.AttachAsyncClick(GitHubClick);
    }

    private void InitializeAnalysisView()
    {
        analysisViewTabs.LoadFromSettings(AppSettings.Instance);
    }

    private void HandleSelectedAnalysisTab()
    {
        ResetHandledCaretPositions();
        analysisViewTabs.SetDefaultsInSettings(AppSettings.Instance);

        var analysisKind = analysisViewTabs.AnalysisNodeKind;
        var analysisView = analysisViewTabs.AnalysisViewKind;
        SetViewVisibility(analysisView);

        if (analysisView is AnalysisViewKind.Tree)
        {
            LoadTreeView(analysisKind);
        }
        else
        {
            LoadDetailsView();
        }
    }

    private void LoadDetailsView()
    {
        var analysisFactory = CreateAnalysisExecutionFactory();
        var analysisExecution = analysisFactory.CreateAnalysisExecution(AnalysisNodeKind.Syntax);
        AnalysisPipelineHandler.AnalysisExecution = analysisExecution;
        AnalysisPipelineHandler.IgnoreInputDelayOnce();
        SetCurrentDetailsView();
    }

    private readonly CancellationTokenFactory _detailsViewCancellationTokenFactory = new();

    private void SetCurrentDetailsView()
    {
        var span = SelectionSpan(codeEditor.textEditor);
        SetCurrentDetailsView(span);
    }

    private void SetCurrentDetailsView(TextSpan span)
    {
        if (AnalysisPipelineHandler.IsWaiting)
            return;

        var currentSource = ViewModel.HybridCompilationSource.CurrentSource;
        var node = currentSource.Tree!.SyntaxNodeAtSpanIncludingStructuredTrivia(span);
        if (node is null)
            return;

        _detailsViewCancellationTokenFactory.Cancel();
        var cancellationToken = _detailsViewCancellationTokenFactory.CurrentToken;

        var analysisRoot = GetNodeViewAnalysisRootForSpan(node, span);

        var execution = new NodeViewAnalysisExecution(currentSource.Compilation, analysisRoot);
        var detailsData = execution.ExecuteCore(cancellationToken);
        if (detailsData is null)
            return;

        _ = coverableView.NodeDetailsView.Load(detailsData);
    }

    private static NodeViewAnalysisRoot GetNodeViewAnalysisRootForSpan(
        SyntaxNode rootNode,
        TextSpan span)
    {
        var token = rootNode.DeepestTokenContainingSpan(span);
        var trivia = rootNode.DeepestTriviaContainingSpan(span);
        return new(rootNode, token, trivia);
    }

    private void LoadTreeView(AnalysisNodeKind analysisKind)
    {
        var analysisFactory = CreateAnalysisExecutionFactory();
        var analysisExecution = analysisFactory.CreateAnalysisExecution(analysisKind);
        AnalysisPipelineHandler.AnalysisExecution = analysisExecution;
        AnalysisPipelineHandler.IgnoreInputDelayOnce();
        if (IsLoaded)
        {
            Task.Run(ForceAnalysisResetTreeView);
        }
    }

    private AnalysisExecutionFactory CreateAnalysisExecutionFactory()
    {
        return new AnalysisExecutionFactory(ViewModel.HybridCompilationSource);
    }

    private void SetViewVisibility(AnalysisViewKind viewKind)
    {
        coverableView.SetContent(viewKind);
    }

    private async Task ForceAnalysisResetTreeView()
    {
        await Task.Run(AnalysisPipelineHandler.ForceAnalysis);
    }

    private void GitHubClick()
    {
        const string githubLink = "https://github.com/Rekkonnect/Syndiesis";
        ProcessUtilities.OpenUrl(githubLink)
            .AwaitProcessInitialized();
    }

    private void CollapseAllClick(object? sender, RoutedEventArgs e)
    {
        coverableView.ListView.ResetToInitialRootView();
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

    private void HandleSelectionChanged(object? sender, EventArgs e)
    {
        RefreshCaretPosition();
    }

    private void HandleCaretPositionChanged(object? sender, EventArgs e)
    {
        RefreshCaretPosition();
    }

    private int _caretPosition = -1;
    private int _selectionLength = -1;

    private volatile bool _pendingDocumentAnalysis = false;

    private void RefreshCaretPosition()
    {
        // Avoid triggering this more than once
        if (_pendingDocumentAnalysis)
            return;

        var position = codeEditor.textEditor.CaretOffset;
        var selectionLength = codeEditor.textEditor.SelectionLength;

        if (position == _caretPosition && selectionLength == _selectionLength)
            return;

        _caretPosition = position;
        _selectionLength = selectionLength;

        var span = SelectionSpan(codeEditor.textEditor);
        ShowCurrentCursorPosition(span);
    }

    private static TextSpan SelectionSpan(TextEditor editor)
    {
        var start = editor.SelectionStart;
        var length = editor.SelectionLength;
        return new(start, length);
    }

    private void ShowCurrentCursorPosition(TextSpan span)
    {
        var analysisView = analysisViewTabs.AnalysisViewKind;
        switch (analysisView)
        {
            case AnalysisViewKind.Tree:
                ShowCurrentCursorPositionForTree(span);
                break;
            case AnalysisViewKind.Details:
                ShowCurrentCursorPositionForDetails(span);
                break;
        }
    }

    private void ShowCurrentCursorPositionForTree(TextSpan span)
    {
        Dispatcher.UIThread.InvokeAsync(()
            => coverableView.ListView.EnsureHighlightedPositionRecurring(span));
    }

    private void ShowCurrentCursorPositionForDetails(TextSpan span)
    {
        SetCurrentDetailsView(span);
    }

    private void HandleHoveredNode(AnalysisTreeListNode? node)
    {
        codeEditor.ShowHoveredSyntaxNode(node ?? _caretHoveredNode);
    }

    private void HandleDetailsViewHoveredNode(AnalysisTreeListNode? node)
    {
        var spanNode = coverableView.NodeDetailsView.NodeForShowingHoverSpan(node);
        HandleHoveredNode(spanNode);
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
        ResetHandledCaretPositions();
    }

    private void ResetHandledCaretPositions()
    {
        _caretPosition = -1;
        _selectionLength = -1;
    }

    private void TriggerPipeline()
    {
        _pendingDocumentAnalysis = true;
        var currentSource = ViewModel.Document;
        AnalysisPipelineHandler.InitiateAnalysis(currentSource);
    }

    public void ApplyCurrentSettings()
    {
        ApplyCurrentSettingsWithoutAnalysis();
        ForceRedoAnalysis();
    }

    private void ApplyCurrentSettingsWithoutAnalysis()
    {
        var settings = AppSettings.Instance;
        AnalysisPipelineHandler.UserInputDelay = settings.UserInputDelay;

        codeEditor.ApplySettings(settings);
    }

    private const string defaultCodeCS = """
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

    private const string defaultCodeVB = """
        #Const SYMVBIOSIS = True

        Imports System

        Namespace Example

            Public Class Program

                Public Shared Sub Main()
                    ' using conditional compilation symbols is fun
        #If SYMVBIOSIS
                    Const greetings As String = "Hello SymVBiosis!"
        #Else
                    Const greetings As String = "Hello World!"
        #End If

                    Console.WriteLine(greetings)
                End Sub

            End Class

        End Namespace

        """;

    public void Reset()
    {
        LoggerExtensionsEx.LogMethodInvocation(nameof(Reset));
        var name = ViewModel.HybridCompilationSource.CurrentLanguageName;
        ResetToLanguage(name);
    }

    public void ResetToLanguage(string languageName)
    {
        LoggerExtensionsEx.LogMethodInvocation($"{nameof(ResetToLanguage)}({languageName})");
        var defaultCode = DefaultCode(languageName);
        SetSource(defaultCode);
        codeEditor.DiagnosticsEnabled = AppSettings.Instance.DiagnosticsEnabled;
    }

    public string ToggleLanguage()
    {
        var current = ViewModel.HybridCompilationSource.CurrentLanguageName;
        var toggled = ToggleLanguageName(current);
        ResetToLanguage(toggled);
        return toggled;
    }

    public void SetLanguageVersion(RoslynLanguageVersion version)
    {
        ViewModel.HybridCompilationSource.SetLanguageVersion(version);

        var newLanguageName = version.LanguageName;
        var currentLanguageName = ViewModel.HybridCompilationSource.CurrentLanguageName;
        if (newLanguageName != currentLanguageName)
        {
            ResetToLanguage(newLanguageName);
        }
        else
        {
            ForceRedoAnalysis();
        }
    }

    private static string ToggleLanguageName(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => LanguageNames.VisualBasic,
            LanguageNames.VisualBasic => LanguageNames.CSharp,
            _ => throw RoslynExceptions.ThrowInvalidLanguageArgument(languageName, nameof(languageName)),
        };
    }

    private static string DefaultCode(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => defaultCodeCS,
            LanguageNames.VisualBasic => defaultCodeVB,
            _ => throw RoslynExceptions.ThrowInvalidLanguageArgument(languageName, nameof(languageName)),
        };
    }

    private void SetSource(string source)
    {
        var analysisPipelineHandler = AnalysisPipelineHandler;

        analysisPipelineHandler.IgnoreInputDelayOnce();
        codeEditor.SetSource(source);
    }

    public void ForceRedoAnalysis()
    {
        ResetHandledCaretPositions();
        var analysisPipelineHandler = AnalysisPipelineHandler;

        analysisPipelineHandler.IgnoreInputDelayOnce();
        TriggerPipeline();
    }

    private void ShowResetSettingsPopup()
    {
        var notificationContainer = ToastNotificationContainer.GetFromMainWindowTopLevel(this);
        if (notificationContainer is not null)
        {
            var popup = new ToastNotificationPopup();
            popup.defaultTextBlock.Text = "Reverted settings to current file state";
            var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(2));
            _ = notificationContainer.Show(popup, animation);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();

        switch (e.Key)
        {
            case Key.S:
                if (modifiers is KeyModifiers.Control)
                {
                    RequestSettings();
                    e.Handled = true;
                }
                break;

            case Key.R:
                if (modifiers is KeyModifiers.Control)
                {
                    Reset();
                    e.Handled = true;
                }
                if (modifiers is (KeyModifiers.Control | KeyModifiers.Shift))
                {
                    ApplyCurrentSettings();
                    ShowResetSettingsPopup();
                    e.Handled = true;
                }
                break;

            // Override Ctrl+Tab to quickly focus on the text area
            // This is also a patch to the inability to gracefully handle automatic
            // focus on the text area, for a variety of focus bugs occurring in AvaloniaEdit
            case Key.Tab:
                if (modifiers is KeyModifiers.Control)
                {
                    codeEditor.textEditor.Focus();
                    e.Handled = true;
                }
                break;
        }

        base.OnKeyDown(e);
    }
}

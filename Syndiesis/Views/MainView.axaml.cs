using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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

        codeEditor.Document = ViewModel.Document;
        codeEditor.SetSource(initializingSource);
        codeEditor.CaretPosition = new(4, 48);
        codeEditor.AssociatedTreeView = syntaxTreeView.listView;

        codeEditor.CompilationSource = ViewModel.HybridCompilationSource;

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
        codeEditor.TextChanged += HandleCodeChanged;
        codeEditor.CaretMoved += HandleCaretPositionChanged;
        codeEditor.SelectionChanged += HandleSelectionChanged;
        syntaxTreeView.listView.HoveredNode += HandleHoveredNode;
        syntaxTreeView.listView.RequestedPlaceCursorAtNode += HandleRequestedPlaceCursorAtNode;
        syntaxTreeView.listView.RequestedSelectTextAtNode += HandleRequestedSelectTextAtNode;
        syntaxTreeView.listView.NewRootLoaded += HandleNewRootNodeLoaded;

        codeEditor.RegisterAnalysisPipelineHandler(AnalysisPipelineHandler);
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
        var analysisFactory = new AnalysisExecutionFactory(ViewModel.HybridCompilationSource);
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

    private void HandleSelectionChanged(object? sender, EventArgs e)
    {
        RefreshCaretPosition();
    }

    private void HandleCaretPositionChanged(object? sender, EventArgs e)
    {
        RefreshCaretPosition();
    }

    private void RefreshCaretPosition()
    {
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
        Dispatcher.UIThread.InvokeAsync(()
            => syntaxTreeView.listView.EnsureHighlightedPositionRecurring(span));
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

                Public Sub Main()

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
    }

    public string ToggleLanguage()
    {
        var current = ViewModel.HybridCompilationSource.CurrentLanguageName;
        var toggled = ToggleLanguageName(current);
        ResetToLanguage(toggled);
        return toggled;
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

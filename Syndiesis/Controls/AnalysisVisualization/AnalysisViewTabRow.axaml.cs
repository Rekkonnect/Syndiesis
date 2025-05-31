using Syndiesis.Controls.Tabs;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class AnalysisViewTabRow : UserControl
{
    private AnalysisNodeKind _analysisNodeKind;
    private AnalysisViewKind _analysisViewKind;

    public AnalysisViewKind AnalysisViewKind
    {
        get => _analysisViewKind;
        set
        {
            var envelope = analysisAlternateViewTabs.Envelopes.FirstOrDefault(
                s => s.Tag?.Equals(value) ?? false);

            analysisAlternateViewTabs.SelectIndex(envelope?.Index);
        }
    }

    public AnalysisNodeKind AnalysisNodeKind
    {
        get => _analysisNodeKind;
        set
        {
            var envelope = analysisTreeViewTabs.Envelopes.FirstOrDefault(
                s => s.Tag?.Equals(value) ?? false);

            analysisTreeViewTabs.SelectIndex(envelope?.Index);
        }
    }

    public event Action? TabSelected;

    public AnalysisViewTabRow()
    {
        InitializeComponent();
        InitializeTabs();
    }

    public void LoadFromSettings(AppSettings settings)
    {
        AnalysisNodeKind = settings.DefaultAnalysisTab;
        AnalysisViewKind = settings.DefaultAnalysisView;
    }

    public void SetDefaultsInSettings(AppSettings settings)
    {
        settings.DefaultAnalysisTab = _analysisNodeKind;
        settings.DefaultAnalysisView = _analysisViewKind;
    }

    private void InitializeTabs()
    {
        analysisTreeViewTabs.Envelopes =
        [
            Envelope("Syntax", AnalysisNodeKind.Syntax),
            Envelope("Symbols", AnalysisNodeKind.Symbol),
            Envelope("Operations", AnalysisNodeKind.Operation),
            Envelope("Attributes", AnalysisNodeKind.Attribute),
        ];

        analysisTreeViewTabs.TabSelected += HandleSelectedAnalysisTreeTab;

        analysisAlternateViewTabs.Envelopes =
        [
            new TabEnvelope
            {
                Text = "Details",
                MinWidth = 95,
                Tag = AnalysisViewKind.Details,
            }
        ];

        analysisAlternateViewTabs.TabSelected += HandleSelectedAnalysisAlternateTab;

        static TabEnvelope Envelope(string text, AnalysisNodeKind analysisKind)
        {
            return new()
            {
                Text = text,
                MinWidth = 95,
                Tag = analysisKind,
            };
        }
    }

    private void HandleSelectedAnalysisTreeTab(TabEnvelope envelope)
    {
        switch (envelope.Tag)
        {
            case AnalysisNodeKind analysisNodeKind:
            {
                _analysisNodeKind = analysisNodeKind;
                _analysisViewKind = AnalysisViewKind.Tree;
                break;
            }
        }
        analysisAlternateViewTabs.SelectIndex(null);
        TabSelected?.Invoke();
    }

    private void HandleSelectedAnalysisAlternateTab(TabEnvelope envelope)
    {
        switch (envelope.Tag)
        {
            case AnalysisViewKind analysisViewKind:
            {
                _analysisNodeKind = AnalysisNodeKind.None;
                _analysisViewKind = analysisViewKind;
                break;
            }
        }
        analysisTreeViewTabs.SelectIndex(null);
        TabSelected?.Invoke();
    }
}

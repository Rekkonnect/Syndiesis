using Avalonia.Input;
using Syndiesis.Utilities;

namespace Syndiesis.Controls.Tabs;

public partial class TabEnvelopeRow : UserControl
{
    private List<TabEnvelope> _tabEnvelopes = new();

    public IReadOnlyList<TabEnvelope> Envelopes
    {
        get => _tabEnvelopes;
        set
        {
            foreach (var oldEnvelope in _tabEnvelopes)
            {
                oldEnvelope.PointerPressed -= HandleSelectionPress;
            }

            _tabEnvelopes = value.ToList();
            envelopesStack.Children.ClearSetValues(value);

            for (int i = 0; i < _tabEnvelopes.Count; i++)
            {
                var envelope = _tabEnvelopes[i];
                envelope.Index = i;
                envelope.PointerPressed += HandleSelectionPress;
            }
        }
    }

    private int _selectedIndex = -1;

    public bool HasSelection => SelectedTab is not null;

    private TabEnvelope? SelectedTab => _tabEnvelopes.ValueAtOrDefault(_selectedIndex);

    public event Action<TabEnvelope>? TabSelected;

    public TabEnvelopeRow()
    {
        InitializeComponent();
    }

    public void SelectIndex(int? index)
    {
        SelectIndex(index ?? -1);
    }

    public void SelectIndex(int index)
    {
        var selected = SelectedTab;
        var next = _tabEnvelopes.ValueAtOrDefault(index);
        if (selected == next)
            return;

        SetCurrentSelection(false);
        _selectedIndex = index;
        SetCurrentSelection(true);

        if (next is not null)
        {
            TabSelected?.Invoke(next);
        }
    }

    private void Select(TabEnvelope envelope)
    {
        SelectIndex(envelope.Index);
    }

    private void SetCurrentSelection(bool selected)
    {
        var tab = SelectedTab;
        if (tab is not null)
        {
            tab.IsSelected = selected;
        }
    }

    private void HandleSelectionPress(object? sender, PointerPressedEventArgs args)
    {
        var properties = args.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed)
            return;

        var envelopeSender = sender as TabEnvelope;
        Debug.Assert(envelopeSender is not null);
        Select(envelopeSender);
    }
}

using Avalonia.Controls;
using System;

namespace Syndiesis.Controls.Tabs;

public partial class TabEnvelope : UserControl
{
    private bool _isSelected = false;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateLowerBorder();
        }
    }

    public string? Text
    {
        get => text.Text;
        set => text.Text = value;
    }

    public int Index { get; set; }

    [Obsolete($"Use {nameof(Tag)}")]
    public object? TagValue { get; set; }

    public TabEnvelope()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateLowerBorder();
    }

    private void UpdateLowerBorder()
    {
        double bottom = GetRequiredLowerBorderMarginBottom();
        lowerBorder.Margin = lowerBorder.Margin.WithBottom(bottom);
    }

    private double GetRequiredLowerBorderMarginBottom()
    {
        return _isSelected ? Bounds.Height : 0;
    }
}

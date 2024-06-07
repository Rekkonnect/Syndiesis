using Avalonia.Controls;
using Avalonia.Interactivity;
using Syndiesis.Core;
using System;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownItem : UserControl
{
    private RoslynLanguageVersion _version;

    public RoslynLanguageVersion Version
    {
        get => _version;
        set
        {
            _version = value;
            TextBlock.Text = value.DisplayVersionNumber();
        }
    }

    public event EventHandler<RoutedEventArgs>? Clicked
    {
        add => Button.Click += value;
        remove => Button.Click -= value;
    }

    public LanguageVersionDropDownItem()
    {
        InitializeComponent();
    }

    public LanguageVersionDropDownItem(RoslynLanguageVersion version)
        : this()
    {
        Version = version;
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Syndiesis.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        mainView.Reset();
        mainView.Focus();
    }
}

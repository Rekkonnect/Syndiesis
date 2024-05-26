using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Syndiesis.Controls;

public partial class SyndiesisTitleBarButtonPanel : UserControl
{
    private static readonly IBrush _whiteBrush = new SolidColorBrush(Colors.White);

    private Window? WindowRoot => VisualRoot as Window;

    public SyndiesisTitleBarButtonPanel()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        minimizeButton.Click += OnMinimizeClick;
        maximizeButton.Click += OnMaximizeClick;
        closeButton.Click += OnCloseClick;
    }

    private void OnMinimizeClick(object? sender, RoutedEventArgs e)
    {
        var window = WindowRoot;
        if (window is null)
            return;

        window.WindowState = WindowState.Minimized;
    }

    private void OnMaximizeClick(object? sender, RoutedEventArgs e)
    {
        var window = WindowRoot;
        if (window is null)
            return;

        window.InvertMaximizedWindowState();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        var window = WindowRoot;
        if (window is null)
            return;

        window.Close();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        InitializeIcons();
        SubscribeToWindowState();
    }

    private void SubscribeToWindowState()
    {
        var window = WindowRoot!;
        window.GetObservable(Window.WindowStateProperty)
            .Subscribe(HandleWindowState);
    }

    private void HandleWindowState(WindowState state)
    {
        var window = WindowRoot!;
        switch (state)
        {
            case WindowState.Maximized:
                SetMaximizeButton(App.Current.ResourceManager.RestoreDownIconGeometry);
                window.Padding = window.SpeculatedOffScreenMargin();
                window.ExtendClientAreaTitleBarHeightHint = Bounds.Height + 2 * window.Padding.Top;
                break;

            default:
                SetMaximizeButton(App.Current.ResourceManager.MaximizeIconGeometry);
                window.Padding = new(0);
                window.ExtendClientAreaTitleBarHeightHint = Bounds.Height;
                break;
        }
    }

    private void SetMaximizeButton(Geometry geometry)
    {
        maximizeButton.PathData = geometry;
    }

    private void InitializeIcons()
    {
        minimizeButton.PathData = App.Current.ResourceManager.MinimizeIconGeometry;
        maximizeButton.PathData = App.Current.ResourceManager.MaximizeIconGeometry;
        closeButton.PathData = App.Current.ResourceManager.CloseIconGeometry;
    }

    private static Path Path(Geometry geometry)
    {
        return new()
        {
            Data = geometry,
            Stretch = Stretch.Uniform,
            Fill = _whiteBrush,
            MaxWidth = 13,
        };
    }
}

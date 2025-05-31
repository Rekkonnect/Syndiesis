using Avalonia.Input;

namespace Syndiesis.Controls;

public partial class SyndiesisTitleBarButton : UserControl
{
    public Geometry PathData
    {
        get => iconPath.Data;
        set => iconPath.Data = value;
    }

    public IBrush? ButtonBrush
    {
        get => button.Background;
        set => button.Background = value;
    }

    public event EventHandler<RoutedEventArgs> Click
    {
        add => button.Click += value;
        remove => button.Click -= value;
    }

    public SyndiesisTitleBarButton()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        button.PointerEntered += HandleButtonPointerEntered;
        button.PointerExited += HandleButtonPointerExited;
    }

    private void HandleButtonPointerEntered(object? sender, PointerEventArgs e)
    {
        UpdateHoverState();
    }

    private void HandleButtonPointerExited(object? sender, PointerEventArgs e)
    {
        UpdateHoverState();
    }

    private static Color _hoverRectangleColor = Color.FromUInt32(0x30FFFFFF);
    private static readonly SolidColorBrush _hoverRectangleBrush = new(_hoverRectangleColor);

    private static readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);

    private void UpdateHoverState()
    {
        var hoverBrush = button.IsPointerOver
            ? _hoverRectangleBrush
            : _transparentBrush;
        hoverRectangle.Fill = hoverBrush;
    }
}

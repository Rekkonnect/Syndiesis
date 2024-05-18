using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls;

public partial class SyndiesisTitleBar : UserControl
{
    private Run _versionRun;
    private Run _commitRun;

    private Window? WindowRoot => VisualRoot as Window;
    private CancellationTokenFactory _pulseLineCancellationTokenFactory = new();

    private PixelPoint _windowStartPosition;
    private PixelPoint _dragStartPosition;
    private PixelPoint _previousDragPosition;

    private readonly PointerDragHandler _dragHandler = new();
    private DragHandling _dragHandling = DragHandling.Enabled;

    public SyndiesisTitleBar()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _dragHandler.DragStarted += RegisterDragStart;
        _dragHandler.Dragged += MoveWindowPosition;
        AddPointerHandlers(lineRectangle);
        AddPointerHandlers(contentStackPanel);
    }

    private void AddPointerHandlers(Control control)
    {
        _dragHandler.Attach(control);
        control.DoubleTapped += HandleTopDoubleTapped;
        control.PointerPressed += HandleLineTapped;
    }

    private void RegisterDragStart(Point point)
    {
        switch (_dragHandling)
        {
            case DragHandling.DisabledNext:
                _dragHandling = DragHandling.Enabled;
                break;

            case DragHandling.Disabled:
                return;
        }

        var window = WindowRoot;
        if (window is null)
            return;

        if (window.WindowState is WindowState.Maximized)
            return;

        _dragStartPosition = window.PointToScreen(point);
        _windowStartPosition = window.Position;
    }

    private void MoveWindowPosition(PointerDragHandler.PointerDragArgs obj)
    {
        switch (_dragHandling)
        {
            case not DragHandling.Enabled:
                return;
        }

        if (obj.Delta is (0, 0))
            return;

        if (obj.SourcePointerEventArgs.KeyModifiers is not KeyModifiers.None)
            return;

        var window = WindowRoot;
        if (window is null)
            return;

        var currentPoint = window.PointToScreen(obj.CurrentPoint);
        if (_previousDragPosition == currentPoint)
            return;

        var offset = currentPoint - _dragStartPosition;
        window.Position = _windowStartPosition + offset;
        _previousDragPosition = currentPoint;

        if (window.WindowState is WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
        }
    }

    private void HandleTopDoubleTapped(object? sender, TappedEventArgs e)
    {
        var window = WindowRoot;
        if (window is null)
            return;

        if (e.KeyModifiers is not KeyModifiers.None)
            return;

        e.Handled = true;
        window.InvertMaximizedWindowState();
        _dragHandling = DragHandling.DisabledNext;
    }

    private void HandleLineTapped(object? sender, PointerEventArgs e)
    {
        var pointer = e.GetCurrentPoint(this);
        if (pointer.Properties.IsLeftButtonPressed)
        {
            switch (e.KeyModifiers.NormalizeByPlatform())
            {
                case KeyModifiers.Control:
                    CopyEntireLine();
                    break;
            }
        }
    }

    private void CopyEntireLine()
    {
        var text = headerText.Inlines!.Text;
        _ = this.SetClipboardTextAsync(text)
            .ConfigureAwait(false);
        PulseCopiedLine();

        var toastContainer = ToastNotificationContainer.GetFromMainWindowTopLevel(this);
        if (toastContainer is not null)
        {
            var popup = new ToastNotificationPopup();
            popup.defaultTextBlock.Text = $"""
                Copied entire line content:
                {text}
                """;
            var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(2));
            _ = toastContainer.Show(popup, animation);
        }
    }

    private void PulseCopiedLine()
    {
        _pulseLineCancellationTokenFactory.Cancel();
        var animation = Animations.CreateOpacityPulseAnimation(lineRectangle, 1, OpacityProperty);
        animation.Duration = TimeSpan.FromMilliseconds(750);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(lineRectangle, _pulseLineCancellationTokenFactory.CurrentToken);
    }

    [MemberNotNull(nameof(_versionRun))]
    [MemberNotNull(nameof(_commitRun))]
    private void InitializeRuns()
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        var version = infoVersion.Version;
        var sha = infoVersion.CommitSha?[..7];

        var groups = new RunOrGrouped[]
        {
            new SingleRunInline(
                new Run("Syndiesis")
                {
                    FontSize = 20,
                }
            ),

            new Run("  "),

            new ComplexGroupedRunInline(
            [
                new Run("v")
                {
                    FontSize = 16,
                    Foreground = new SolidColorBrush(0xFF808080),
                },

                new SingleRunInline(
                    _versionRun = new Run(version)
                    {
                        FontSize = 18,
                    }
                ),
            ]),

            new Run("  "),

            new ComplexGroupedRunInline(
            [
                new Run("commit ")
                {
                    FontSize = 14,
                    Foreground = new SolidColorBrush(0xFF808080),
                },

                new SingleRunInline(
                    _commitRun = new Run(sha)
                    {
                        FontSize = 18,
                    }
                ),
            ]),

#if DEBUG
            new Run("   "),

            new SingleRunInline(
                new Run("[DEBUG]")
                {
                    FontSize = 18,
                    Foreground = new SolidColorBrush(0xFF00A0AA),
                }
            ),
#endif
        };

        if (sha is null)
        {
            _commitRun.Foreground = new SolidColorBrush(0xFF440015);
            _commitRun.Text = "[MISSING]";
        }

        headerText.GroupedRunInlines = new(groups);
    }
}

using Avalonia.Animation.Easings;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls;

public partial class SyndiesisTitleBar : UserControl
{
    private Run _versionRun;
    private Run _commitRun;

    private Window? WindowRoot => VisualRoot as Window;
    private Run _titleRun;
    private ReusableCancellableAnimation _pulseLineAnimation;

    public Bitmap LogoImage
    {
        get => (Bitmap)logoImage.Source!;
    }

    public event EventHandler<PointerPressedEventArgs> LogoClicked
    {
        add => logoImage.PointerPressed += value;
        remove => logoImage.PointerPressed -= value;
    }

    public SyndiesisTitleBar()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();
        InitializeAnimations();
    }

    private void InitializeEvents()
    {
        AddPointerHandlers(linePulseRectangle);
        AddPointerHandlers(contentStackPanel);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var windowRoot = WindowRoot!;
        windowRoot.KeyDown += HandleKeyDown;
        windowRoot.KeyUp += HandleKeyUp;
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.NormalizeByPlatform().HasFlag(KeyModifiers.Control))
        {
            SetHitTest(false);
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.NormalizeByPlatform().HasFlag(KeyModifiers.Control))
        {
            SetHitTest(true);
        }
    }

    private void SetHitTest(bool value)
    {
        lineBackground.IsHitTestVisible = value;
        linePulseRectangle.IsHitTestVisible = value;
        headerText.IsHitTestVisible = value;
    }

    private void AddPointerHandlers(Control control)
    {
        control.PointerPressed += HandleLineTapped;
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

        var toastContainer = ToastNotificationContainer.GetFromOuterMainViewContainer();
        if (toastContainer is not null)
        {
            var popupContent = $"""
                Copied entire line content:
                {text}
                """;
            _ = CommonToastNotifications.ShowClassicMain(
                toastContainer,
                popupContent,
                TimeSpan.FromSeconds(2));
        }
    }

    private void PulseCopiedLine()
    {
        _ = _pulseLineAnimation.RunAsync(linePulseRectangle);
    }

    [MemberNotNull(nameof(_pulseLineAnimation))]
    private void InitializeAnimations()
    {
        _pulseLineAnimation = CreatePulseAnimation();
    }

    private ReusableCancellableAnimation CreatePulseAnimation()
    {
        var animation = Animations.CreatePropertyPulseAnimation(
            linePulseRectangle, 1, OpacityProperty);
        animation.Duration = TimeSpan.FromMilliseconds(750);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        return new(animation);
    }

    [MemberNotNull(nameof(_titleRun))]
    [MemberNotNull(nameof(_versionRun))]
    [MemberNotNull(nameof(_commitRun))]
    private void InitializeRuns()
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        var version = infoVersion.Version;
        var sha = infoVersion.CommitSha?.Short;

        var groups = new RunOrGrouped[]
        {
            new SingleRunInline(
                _titleRun = new Run("Syndiesis")
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

    private static readonly Color _csBackground = Color.FromUInt32(0xFF004044);
    private static readonly Color _vbBackground = Color.FromUInt32(0xFF104565);

    public void SetThemeForLanguage(string languageName)
    {
        switch (languageName)
        {
            case LanguageNames.CSharp:
                lineBackground.Fill = new SolidColorBrush(_csBackground);
                _titleRun.Text = "Syndiesis";
                logoImage.Source = App.CurrentResourceManager.LogoCSImage!.Source;
                break;

            case LanguageNames.VisualBasic:
                lineBackground.Fill = new SolidColorBrush(_vbBackground);
                _titleRun.Text = "SymVBiosis";
                logoImage.Source = App.CurrentResourceManager.LogoVBImage!.Source;
                break;
        }

        headerText.Redraw();
    }
}

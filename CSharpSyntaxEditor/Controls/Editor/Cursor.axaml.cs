using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using CSharpSyntaxEditor.Utilities;
using System;

namespace CSharpSyntaxEditor.Controls;

public partial class Cursor : UserControl
{
    private static readonly Animation _cursorBlinkAnimation = CreateCursorBlinkAnimation();

    private readonly AnimationRunController _cursorBlinkAnimationController = new(_cursorBlinkAnimation);

    public double LeftOffset
    {
        get => Padding.Left;
        set => Padding = Padding.WithLeft(value);
    }

    public Cursor()
    {
        InitializeComponent();
        Hide();
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Hide()
    {
        IsVisible = false;
        StopAnimation();
    }

    public void Show()
    {
        IsVisible = true;
        RunAnimation();
    }

    public void RestartAnimation()
    {
        StopAnimation();
        RunAnimation();
    }

    public void RunAnimation()
    {
        _ = _cursorBlinkAnimationController.RunAsync(cursorRectangle);
    }

    public void StopAnimation()
    {
        _cursorBlinkAnimationController.Stop();
    }

    private static Animation CreateCursorBlinkAnimation()
    {
        return new()
        {
            Duration = TimeSpan.FromMilliseconds(1000),
            IterationCount = AnimationConveniences.NearInfiniteIterationCount,
            Children =
            {
                CreateFillKeyFrame(0.00, Colors.White),
                CreateFillKeyFrame(0.50, Colors.White),
                CreateFillKeyFrame(0.65, Color.FromArgb(128, 100, 100, 100)),
                CreateFillKeyFrame(1.00, Color.FromArgb(128, 100, 100, 100)),
            },
        };

        static KeyFrame CreateFillKeyFrame(double cue, Color color)
        {
            return new()
            {
                Cue = new Cue(cue),
                Setters =
                {
                    new Setter
                    {
                        Property = Rectangle.FillProperty,
                        Value = new SolidColorBrush(color),
                    }
                }
            };
        }
    }

    public void SetStaticColor(Color color)
    {
        StopAnimation();
        cursorRectangle.Fill = new SolidColorBrush(color);
    }
}

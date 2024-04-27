using Avalonia;
using Avalonia.Controls;
using CSharpSyntaxEditor.Utilities;
using System;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditorLineDisplayPanel : UserControl
{
    private bool _pendingRender;

    public static readonly StyledProperty<int> SelectedLineNumberProperty =
        AvaloniaProperty.Register<CodeEditorLineDisplayPanel, int>(nameof(SelectedLineNumber), defaultValue: 1);

    public int SelectedLineNumber
    {
        get => GetValue(SelectedLineNumberProperty);
        set
        {
            int previousValue = SelectedLineNumber;
            if (previousValue == value)
                return;

            SetValue(SelectedLineNumberProperty, value);

            var previousLine = LineWithNumber(previousValue);
            var currentLine = LineWithNumber(value);

            if (previousLine is not null)
                previousLine.SelectedLine = false;

            if (currentLine is not null)
                currentLine.SelectedLine = true;
        }
    }

    public static readonly StyledProperty<int> LineNumberStartProperty =
        AvaloniaProperty.Register<CodeEditorLineDisplayPanel, int>(nameof(LineNumberStart), defaultValue: 1);

    public int LineNumberStart
    {
        get => GetValue(LineNumberStartProperty);
        set
        {
            int previousValue = LineNumberStart;
            if (previousValue == value)
                return;

            SetValue(LineNumberStartProperty, value);

            RequestRender();
        }
    }

    public static readonly StyledProperty<int> LastLineNumberProperty =
        AvaloniaProperty.Register<CodeEditorLineDisplayPanel, int>(nameof(LastLineNumber), defaultValue: 1);

    public int LastLineNumber
    {
        get => GetValue(LastLineNumberProperty);
        set
        {
            int previousValue = LastLineNumber;
            if (previousValue == value)
                return;

            SetValue(LastLineNumberProperty, value);

            RequestRender();
        }
    }

    private static int GetVisibleLineCount(double height)
    {
        return (int)(height / CodeEditor.LineHeight) + 1;
    }

    private void EnsureEnoughVisibleLineNumbers(double height)
    {
        int lineCount = GetVisibleLineCount(height);
        EnsureEnoughLineNumbers(lineCount);
    }

    private void EnsureEnoughLineNumbers(int numbers)
    {
        int children = lineNumbersPanel.Children.Count;

        for (; children < numbers; children++)
        {
            lineNumbersPanel.Children.Add(new CodeEditorLineNumber());
        }
    }

    public void ForceRender()
    {
        _pendingRender = true;
        ApplyRequestedRender(Bounds.Height);
    }

    private void RequestRender()
    {
        _pendingRender = true;
    }

    private void ApplyRequestedRender(double height)
    {
        if (_pendingRender)
        {
            RenderLineNumbers(height);
            _pendingRender = false;
        }
    }

    private void RenderLineNumbers(double height)
    {
        int lineStart = LineNumberStart;
        EnsureEnoughVisibleLineNumbers(height);
        int visibleLines = GetVisibleLineCount(height);
        int lastVisible = lineStart + visibleLines - 1;
        int lineEnd = Math.Min(lastVisible, LastLineNumber);

        int selectedNumber = SelectedLineNumber;
        for (int i = 0; i < visibleLines; i++)
        {
            int number = lineStart + i;
            var line = LineAtIndex(i)!;
            line.SelectedLine = number == selectedNumber;
            line.LineNumber = number;
            line.IsVisible = number <= lineEnd;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        RenderLineNumbers((int)e.NewSize.Height);
        base.OnSizeChanged(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        ApplyRequestedRender((int)availableSize.Height);
        return base.MeasureOverride(availableSize);
    }

    public CodeEditorLineDisplayPanel()
    {
        InitializeComponent();
    }

    private CodeEditorLineNumber? LineWithNumber(int number)
    {
        int index = TranslateLineNumberToIndex(number);
        return LineAtIndex(index);
    }
    private CodeEditorLineNumber? LineAtIndex(int index)
    {
        var children = lineNumbersPanel.Children;
        if (index >= children.Count)
            return null;
        return children.ValueAtOrDefault(index) as CodeEditorLineNumber;
    }
    private int TranslateLineNumberToIndex(int number)
    {
        return number - LineNumberStart;
    }
}

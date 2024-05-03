using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Models;
using Syndiesis.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls;

public partial class CodeEditorLine : UserControl
{
    private static readonly SolidColorBrush _selectedLineBackgroundBrush = new(0x80102020);
    private static readonly SolidColorBrush _unselectedLineBackgroundBrush = new(Colors.Transparent);

    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            lineContentText.Text = value;
        }
    }

    public int TextLength => Text.Length;

    public static readonly StyledProperty<bool> SelectedLineProperty =
        AvaloniaProperty.Register<CodeEditorLine, bool>(nameof(SelectedLine), defaultValue: false);

    public bool SelectedLine
    {
        get => GetValue(SelectedLineProperty);
        set
        {
            SetValue(SelectedLineProperty, value);
            var backgroundBrush = value
                ? _selectedLineBackgroundBrush
                : _unselectedLineBackgroundBrush;

            lineContentPanel.Background = backgroundBrush;
            cursor.SetVisible(value);
        }
    }

    public static readonly StyledProperty<InlineCollection> InlinesProperty =
        AvaloniaProperty.Register<CodeEditorLine, InlineCollection>(nameof(Inlines), defaultValue: []);

    public InlineCollection? Inlines
    {
        get => lineContentText.Inlines;
        set
        {
            SetValue(InlinesProperty!, value!);
            lineContentText.Inlines = value;
        }
    }

    private int _cursorCharacterIndex = 0;

    public int CursorCharacterIndex
    {
        get => _cursorCharacterIndex;
        set
        {
            _cursorCharacterIndex = value;
            double newLeftPosition = CodeEditor.CharacterBeginPosition(value) + 1;
            cursor.LeftOffset = newLeftPosition;
        }
    }

    public HighlightHandler SyntaxNodeHoverHighlight { get; private set; }
    public HighlightHandler SelectionHighlight { get; private set; }

    public CodeEditorLine()
    {
        InitializeComponent();
        InitializeRectangleHandlers();
    }

    [MemberNotNull(nameof(SelectionHighlight))]
    [MemberNotNull(nameof(SyntaxNodeHoverHighlight))]
    private void InitializeRectangleHandlers()
    {
        SyntaxNodeHoverHighlight = new(syntaxNodeHoverHighlight);
        SelectionHighlight = new(selectionHighlight);
    }

    public HighlightHandler GetHighlightHandler(HighlightKind kind)
    {
        return kind switch
        {
            HighlightKind.SyntaxNodeHover => SyntaxNodeHoverHighlight,
            HighlightKind.Selection => SelectionHighlight,
        };
    }

    // this is ready to be used for whenever syntax highlighting is implemented
    public void HighlightText(ReadOnlySpan<LineHighlightRange> sortedHighlights)
    {
        var runs = new InlineCollection();
        int firstUnhandledIndex = 0;
        var text = Text;
        foreach (var highlight in sortedHighlights)
        {
            int start = highlight.Start;
            if (start > firstUnhandledIndex)
            {
                var intermediateSubstring = text[firstUnhandledIndex..start];
                var intermediateRun = new Run(intermediateSubstring);
                runs.Add(intermediateRun);
            }

            int end = highlight.End;
            var substring = text[start..end];
            var highlightRun = new Run(substring)
            {
                Foreground = new SolidColorBrush(highlight.Highlight),
            };

            runs.Add(highlightRun);
        }

        lineContentText.Inlines = runs;
    }

    public void RestartCursorAnimation()
    {
        cursor.RunAnimation();
    }

    public void HideCursor()
    {
        cursor.Hide();
    }

    public void ShowCursor()
    {
        cursor.Show();
    }

    public void StopCursorAnimation()
    {
        cursor.StopAnimation();
    }

    public void SetStaticCursorColor(Color color)
    {
        cursor.SetStaticColor(color);
    }

    public sealed record HighlightHandler(Rectangle HighlightingRectangle)
    {
        private Range? _currentSpan;

        public Range? CurrentSpan
        {
            get => _currentSpan;
            private set
            {
                _currentSpan = value;
                UpdateHighlight();
            }
        }

        private static int Length(CodeEditorLine line, bool includeNewLine)
        {
            int length = line.TextLength;
            if (includeNewLine)
            {
                length++;
            }
            return length;
        }

        public void SetEntireLine(CodeEditorLine line, bool includeNewLine)
        {
            int length = Length(line, includeNewLine);
            Set(0..length);
        }

        public void SetRightPart(int start, CodeEditorLine line, bool includeNewLine)
        {
            int length = Length(line, includeNewLine);
            Set(start..length);
        }

        public void SetLeftPart(int end)
        {
            Set(0..end);
        }

        public void Set(Range span)
        {
            CurrentSpan = span;
        }

        public void Clear()
        {
            CurrentSpan = null;
        }

        private void UpdateHighlight()
        {
            var span = _currentSpan;
            if (span is null)
            {
                HighlightingRectangle.Width = 0;
                return;
            }

            var spanValue = span.Value;
            int start = spanValue.Start.Value;
            int end = spanValue.End.Value;
            if (start == end)
            {
                HighlightingRectangle.Width = 0;
                return;
            }

            var left = CodeEditor.CharacterBeginPosition(start);
            HighlightingRectangle.Margin = HighlightingRectangle.Margin.WithLeft(left);

            var right = CodeEditor.CharacterBeginPosition(end);
            var width = right - left;
            if (width > 0)
            {
                width += 1;
            }

            HighlightingRectangle.Width = width;
        }
    }

    public enum HighlightKind
    {
        SyntaxNodeHover,
        Selection,
    }
}

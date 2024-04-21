using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditorLineNumber : UserControl
{
    private static readonly SolidColorBrush _selectedLineNumberBrush = new(Colors.White);
    private static readonly SolidColorBrush _unselectedLineNumberBrush = new(0xFF9F9F9F);

    public static readonly StyledProperty<bool> SelectedLineProperty =
        AvaloniaProperty.Register<CodeEditorLine, bool>(nameof(SelectedLine), defaultValue: false);

    public bool SelectedLine
    {
        get => GetValue(SelectedLineProperty);
        set
        {
            SetValue(SelectedLineProperty, value);

            var lineNumberBrush = value
                ? _selectedLineNumberBrush
                : _unselectedLineNumberBrush;

            lineNumberText.Foreground = lineNumberBrush;
        }
    }

    public static readonly StyledProperty<int> LineNumberProperty =
        AvaloniaProperty.Register<CodeEditorLine, int>(nameof(LineNumber), defaultValue: 1);

    public int LineNumber
    {
        get => GetValue(LineNumberProperty);
        set
        {
            SetValue(LineNumberProperty, value);
            lineNumberText.Text = value.ToString();
        }
    }

    public CodeEditorLineNumber()
    {
        InitializeComponent();
    }
}

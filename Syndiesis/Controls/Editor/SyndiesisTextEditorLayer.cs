using AvaloniaEdit.Rendering;
using System.Reflection;

namespace Syndiesis.Controls.Editor;

public abstract class SyndiesisTextEditorLayer : Control
{
    public static readonly MethodInfo RenderBackgroundMethod
        = typeof(TextView)
            .GetMethod(
                "RenderBackground",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            ;

    protected readonly CodeEditor CodeEditor;
    protected readonly SyndiesisTextArea TextArea;
    protected readonly SyndiesisTextView TextView;

    protected SyndiesisTextEditorLayer(CodeEditor codeEditor)
    {
        CodeEditor = codeEditor;
        TextArea = codeEditor.textEditor.TextArea;
        TextView = TextArea.TextView;

        IsHitTestVisible = false;

        TextViewWeakEventManager.ScrollOffsetChanged.AddHandler(TextView, ReceiveWeakEvent);
    }

    protected void ReceiveWeakEvent(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }
}

using Avalonia.Controls.Primitives;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using System.Reflection;

namespace Syndiesis.Controls.Editor;

public static class TextViewUnsafeExtensions
{
    private static readonly FieldInfo _heightTreeField;
    private static readonly Type _heightTreeType;
    private static readonly MethodInfo _getLineByVisualPositionMethod;

    static TextViewUnsafeExtensions()
    {
        _heightTreeField =
            typeof(TextView)
                .GetField(
                    "_heightTree",
                    BindingFlags.NonPublic | BindingFlags.Instance)!;

        _heightTreeType = _heightTreeField.FieldType;

        _getLineByVisualPositionMethod =
            _heightTreeType
                .GetMethod(
                    "GetLineByVisualPosition",
                    BindingFlags.Public | BindingFlags.Instance)!;
    }

    private static object GetHeightTree(this TextView textView)
    {
        return _heightTreeField.GetValue(textView)!;
    }

    private static DocumentLine? GetLineByVisualPosition(this TextView textView, double offset)
    {
        var heightTree = GetHeightTree(textView);
        return _getLineByVisualPositionMethod.Invoke(heightTree, [offset])
            as DocumentLine;
    }

    private static int GetLineNumberByVisualPosition(this TextView textView, double offset)
    {
        return GetLineByVisualPosition(textView, offset)?.LineNumber ?? -1;
    }

    public static int GetFirstVisibleLine(this TextView textView)
    {
        var offset = textView.ScrollOffset.Y;
        return GetLineNumberByVisualPosition(textView, offset);
    }

    public static int GetLastVisibleLine(this TextView textView)
    {
        var viewport = (textView as IScrollable).Viewport;
        var offset = textView.ScrollOffset.Y + viewport.Height;
        return GetLineNumberByVisualPosition(textView, offset);
    }
}

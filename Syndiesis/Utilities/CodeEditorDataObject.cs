using Avalonia.Input;

namespace Syndiesis.Utilities;

public class CodeEditorDataObject : DataObject
{
    public void SetLine(string line)
    {
        Set(DataFormats.Text, line);
        Set(Formats.CodeEditor, Formats.CodeEditor);
    }

    public static CodeEditorDataObject ForSingleLine(string line)
    {
        var result = new CodeEditorDataObject();
        result.SetLine(line);
        return result;
    }

    public static class Formats
    {
        public const string CodeEditor = "CodeEditor";
    }
}

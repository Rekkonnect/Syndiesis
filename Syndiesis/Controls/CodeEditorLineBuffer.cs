using Syndiesis.Utilities;
using System;
using System.Collections.Generic;

namespace Syndiesis.Controls;

public class CodeEditorLineBuffer
{
    private readonly List<CodeEditorLine> _lines = new();
    private int _lineOffset;

    public int Capacity => _lines.Count;
    public int LineOffset => _lineOffset;

    public CodeEditorLineBuffer(int defaultCapacity)
    {
        SetCapacity(defaultCapacity);
    }

    public void SetCapacity(int capacity)
    {
        while (capacity > _lines.Count)
        {
            _lines.Add(new());
        }
    }

    public void SetLine(int line, string value)
    {
        int index = line - _lineOffset;
        if (index < 0 || index >= _lines.Count)
            return;

        _lines[index].Text = value;
    }

    public void LoadFrom(int start, MultilineStringEditor sourceEditor)
    {
        _lineOffset = start;
        for (int i = 0; i < _lines.Count; i++)
        {
            int sourceLine = start + i;
            if (sourceLine >= sourceEditor.LineCount)
            {
                ClearLinesFrom(i);
                break;
            }

            var line = _lines[i];
            line.Text = sourceEditor.AtLine(start + i);
        }
    }

    private void ClearLinesFrom(int start)
    {
        int offsetStart = start;
        for (int i = offsetStart; i < _lines.Count; i++)
        {
            var line = _lines[i];
            line.Text = string.Empty;
        }
    }

    public IReadOnlyList<CodeEditorLine> LineSpanForRange(int start, int count)
    {
        int end = start + count;
        int offsetStart = start - _lineOffset;
        int offsetEnd = end - _lineOffset;
        offsetStart = Math.Max(offsetStart, 0);
        offsetEnd = Math.Min(offsetEnd, _lines.Count);

        return _lines[offsetStart..offsetEnd];
    }

    public IReadOnlyList<CodeEditorLine> LineSpanForAbsoluteIndexRange(int start, int count)
    {
        int end = start + count;
        start = Math.Max(start, 0);
        end = Math.Min(end, _lines.Count);

        return _lines[start..end];
    }

    public CodeEditorLine? GetLine(int line)
    {
        int index = line - _lineOffset;
        return _lines.ValueAtOrDefault(index);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSyntaxEditor.Utilities;

public sealed class MultilineStringEditor
{
    private readonly List<string> _lines = new();

    public int LineCount => _lines.Count;

    public void SetText(string text)
    {
        Clear();
        foreach (var line in text.AsSpan().EnumerateLines())
        {
            _lines.Add(line.ToString());
        }
    }

    public void Clear()
    {
        _lines.Clear();
    }

    public void InsertEmptyLineAt(int line)
    {
        InsertLineAt(line, string.Empty);
    }

    public void InsertLineAt(int line, string value)
    {
        _lines.Insert(line, value);
    }

    public void BreakLineAt(int line, int upperLineLength)
    {
        var sourceLine = AtLine(line);
        if (upperLineLength >= sourceLine.Length)
        {
            return;
        }

        var start = sourceLine[..upperLineLength];
        var end = sourceLine[upperLineLength..];
        SetLine(line, start);
        InsertLineAt(line + 1, end);
    }

    // muy bien copypasta

    public void InsertAt(int line, int column, char value)
    {
        var previousLine = AtLine(line);
        var nextLine = previousLine.InsertAt(column, value);
        SetLine(line, nextLine);
    }

    public void InsertAt(int line, int column, string value)
    {
        var previousLine = AtLine(line);

        bool multiline = value.IsMultiline();
        if (!multiline)
        {
            InsertSingleLine();
        }
        else
        {
            InsertMultiLine();
        }

        void InsertSingleLine()
        {
            var nextLine = previousLine.Insert(column, value);
            SetLine(line, nextLine);
        }

        void InsertMultiLine()
        {
            var previousStart = previousLine[..column];
            var previousEnd = previousLine[column..];
            var enumeratedLines = value.AsSpan().EnumerateLines();
            int lineOffset = 0;
            string lastLine = string.Empty;
            foreach (var enumeratedLine in enumeratedLines)
            {
                if (lineOffset is 0)
                {
                    var newStartLine = previousStart + enumeratedLine.ToString();
                    SetLine(line, newStartLine);
                }
                else
                {
                    lastLine = enumeratedLine.ToString();
                    InsertLineAt(line + lineOffset, lastLine);
                }

                lineOffset++;
            }

            if (previousEnd.Length > 0)
            {
                SetLine(line + lineOffset - 1, lastLine + previousEnd);
            }
        }
    }

    public void RemoveAt(int line, int column)
    {
        var previousLine = AtLine(line);
        if (previousLine.Length is 0)
        {
            RemoveLine(line);
            return;
        }

        var nextLine = previousLine.Remove(column, 1);
        SetLine(line, nextLine);
    }

    public void RemoveLine(int line)
    {
        _lines.RemoveAt(line);
    }

    public void RemoveBackwardsAt(int line, int column, int count)
    {
        var previousLine = AtLine(line);
        if (count >= previousLine.Length)
        {
            // we do not mess with the previous lines in this one
            // usually the caller will mind the length of the line
            // if need be, we may change this
            ClearLine(line);
            return;
        }

        var nextLine = previousLine.RemoveBackwards(column, count);
        SetLine(line, nextLine);
    }

    public void RemoveForwardsAt(int line, int column, int count)
    {
        var previousLine = AtLine(line);
        if (count >= previousLine.Length)
        {
            ClearLine(line);
            return;
        }

        var nextLine = previousLine.Remove(column, count);
        SetLine(line, nextLine);
    }

    public void RemoveStart(int line, int column)
    {
        var previousLine = AtLine(line);
        var nextLine = previousLine[column..];
        SetLine(line, nextLine);
    }

    public void RemoveEnd(int line, int column)
    {
        var previousLine = AtLine(line);
        var nextLine = previousLine[..column];
        SetLine(line, nextLine);
    }

    public void RemoveLineRange(int startLine, int endLine)
    {
        int count = endLine - startLine + 1;
        _lines.RemoveRange(startLine, count);
    }

    public void RemoveRangeInLine(int line, int startColumn, int endColumn)
    {
        int count = endColumn - startColumn + 1;
        RemoveForwardsAt(line, startColumn, count);
    }

    public void RemoveRange(int startLine, int endLine, int startColumn, int endColumn)
    {
        int nextStart = startLine + 1;
        int previousEnd = endLine - 1;
        if (nextStart <= previousEnd)
        {
            RemoveLineRange(nextStart, previousEnd);
        }

        if (startLine == endLine)
        {
            RemoveRangeInLine(startLine, startColumn, endColumn);
            return;
        }

        RemoveEnd(startLine, startColumn);
        RemoveStart(endLine, endColumn);
    }

    public void ClearLine(int line)
    {
        SetLine(line, string.Empty);
    }

    public void SetLine(int line, string value)
    {
        _lines[line] = value;
    }

    public string AtLine(int line)
    {
        return _lines[line];
    }

    public string FullString()
    {
        int lineCount = LineCount;

        if (lineCount is 0)
            return string.Empty;

        const int averageLineLength = 30;
        var builder = new StringBuilder(lineCount * averageLineLength);

        for (int i = 0; i < _lines.Count; i++)
        {
            builder.Append(_lines[i]);
            if (i < _lines.Count - 1)
            {
                builder.Append("\r\n");
            }
        }

        return builder.ToString();
    }
}

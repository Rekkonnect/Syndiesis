using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Syndiesis.Utilities;

public sealed class MultilineStringEditor
{
    private const string DefaultNewLine = "\r\n";

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

    public int GetIndex(LinePosition position)
    {
        int line = position.Line;
        if (line >= _lines.Count)
            throw new IndexOutOfRangeException("The given position was out of the bounds of the text");

        const int lineEndingLength = 2; // \r\n
        int index = 0;
        for (int i = 0; i < line; i++)
        {
            index += _lines[i].Length + lineEndingLength;
        }

        return index + position.Character;
    }

    public void InsertEmptyLineAt(int line)
    {
        InsertLineAt(line, string.Empty);
    }

    public void InsertLineAt(int line, string value)
    {
        _lines.Insert(line, value);
    }

    public void InsertLineAtColumn(int line, int column)
    {
        var sourceLine = LineAt(line);
        if (column >= sourceLine.Length)
        {
            InsertEmptyLineAt(line + 1);
            return;
        }

        BreakLineAt(line, column);
    }

    public void BreakLineAt(int line, int upperLineLength)
    {
        var sourceLine = LineAt(line);
        if (upperLineLength >= sourceLine.Length)
        {
            return;
        }

        var start = sourceLine[..upperLineLength];
        var end = sourceLine[upperLineLength..];
        SetLine(line, start);
        InsertLineAt(line + 1, end);
    }

    public void InsertAt(int line, int column, char value)
    {
        var previousLine = LineAt(line);
        var nextLine = previousLine.InsertAt(column, value);
        SetLine(line, nextLine);
    }

    public LinePosition InsertAt(int line, int column, string value)
    {
        var previousLine = LineAt(line);

        bool multiline = value.IsMultiline();
        if (!multiline)
        {
            return InsertSingleLine();
        }
        else
        {
            return InsertMultiLine();
        }

        LinePosition InsertSingleLine()
        {
            var nextLine = previousLine.Insert(column, value);
            SetLine(line, nextLine);
            return new(line, column + value.Length);
        }

        LinePosition InsertMultiLine()
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
                int finalLineIndex = line + lineOffset - 1;
                SetLine(finalLineIndex, lastLine + previousEnd);
                return new(finalLineIndex, lastLine.Length);
            }

            return new(line + lineOffset - 1, lastLine.Length);
        }
    }

    public void RemoveAt(int line, int column)
    {
        var previousLine = LineAt(line);
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
        if (count <= 0)
            return;

        var previousLine = LineAt(line);
        int removedLength = column;
        int remainingLength = previousLine.Length - column;
        if (count >= column)
        {
            count -= column;
            if (count > 0)
            {
                // eat up a virtual newline
                count--;
                MergeLineWithBelow(line - 1);
            }
            else
            {
                var trimmedLine = previousLine[column..];
                SetLine(line, trimmedLine);
            }
            var previousLineIndex = line - 1;
            if (previousLineIndex < 0)
                return;

            var previousLineLength = LineAt(previousLineIndex).Length;
            RemoveBackwardsAt(previousLineIndex, previousLineLength, count);
            return;
        }

        var nextLine = previousLine.RemoveBackwards(column - 1, count);
        SetLine(line, nextLine);
    }

    public void RemoveForwardsAt(int line, int column, int count)
    {
        // TODO FIX BASED ON ABOVE
        if (count <= 0)
            return;

        var previousLine = LineAt(line);
        int removedLength = previousLine.Length - column;
        int remainingLength = column;
        if (count >= removedLength)
        {
            count -= removedLength;
            var nextLineIndex = line + 1;

            if (count > 0)
            {
                // eat up a virtual newline
                count--;
                nextLineIndex--;
                MergeLineWithBelow(line);
            }
            else
            {
                var trimmedLine = previousLine[..column];
                SetLine(line, trimmedLine);
                previousLine = trimmedLine;
            }

            if (nextLineIndex < _lines.Count)
            {
                RemoveForwardsAt(nextLineIndex, 0, count);
            }

            return;
        }

        var nextLine = previousLine.Remove(column, count);
        SetLine(line, nextLine);
    }

    public void MergeLineWithBelow(int line)
    {
        if (line < 0)
            return;

        int lineCount = LineCount;
        if (line >= lineCount)
            return;

        var current = _lines[line];
        var next = _lines[line + 1];
        var set = current + next;
        SetLine(line, set);
        RemoveLine(line + 1);
    }

    public void RemoveStart(int line, int column)
    {
        var previousLine = LineAt(line);
        var nextLine = previousLine[column..];
        SetLine(line, nextLine);
    }

    public void RemoveEnd(int line, int column)
    {
        var previousLine = LineAt(line);
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
        if (startLine == endLine)
        {
            RemoveRangeInLine(startLine, startColumn, endColumn - 1);
            return;
        }

        RemoveEnd(startLine, startColumn);
        RemoveStart(endLine, endColumn);

        int nextStart = startLine + 1;
        int previousEnd = endLine - 1;
        if (nextStart <= previousEnd)
        {
            RemoveLineRange(nextStart, previousEnd);
        }
    }

    public void RemoveNewLineIntoBelow(int line)
    {
        if (line >= _lines.Count - 1)
            return;

        var current = LineAt(line);
        var next = LineAt(line + 1);
        RemoveLine(line + 1);
        SetLine(line, current + next);
    }

    public void ClearLine(int line)
    {
        SetLine(line, string.Empty);
    }

    public void SetLine(int line, string value)
    {
        _lines[line] = value;
    }

    public string LineAt(int line)
    {
        return _lines[line];
    }

    public int LineLength(int line) => _lines[line].Length;

    public string FullString(string newLine = DefaultNewLine)
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
                builder.Append(newLine);
            }
        }

        return builder.ToString();
    }

    public string SectionString(LinePositionSpan span, string newLine = DefaultNewLine)
    {
        var start = span.Start;
        var end = span.End;

        AssertValidPosition(start);
        AssertValidPosition(end);

        if (LineCount is 0)
            return string.Empty;

        if (start.Line == end.Line)
        {
            var singleLine = _lines[start.Line];
            var startChar = start.Character;
            var endChar = end.Character;
            return singleLine[startChar..endChar];
        }

        const int averageLineLength = 30;
        int lineCount = end.Line - start.Line;
        Debug.Assert(lineCount > 0, "invalid span range was given");
        var builder = new StringBuilder((lineCount + 2) * averageLineLength);

        var firstLine = LineAt(start.Line);
        builder.Append(firstLine[start.Character..]);
        builder.Append(newLine);

        for (int i = start.Line + 1; i < end.Line; i++)
        {
            builder.Append(_lines[i]);
            builder.Append(newLine);
        }

        var lastLine = LineAt(end.Line);
        builder.Append(lastLine[..end.Character]);

        return builder.ToString();
    }

    [Conditional("DEBUG")]
    public void AssertValidPosition(LinePosition position)
    {
        var line = position.Line;
        var column = position.Character;

        Debug.Assert(0 <= line && line < _lines.Count);

        var length = LineLength(line);

        Debug.Assert(0 <= column && column <= length);
    }

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"Prefer {nameof(FullString)}")]
    public override string ToString()
    {
        return base.ToString()!;
    }
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
}

using System;

namespace Syndiesis.Utilities;

public ref struct HexIntegerWriter(Span<char> buffer, int groupLength = 0)
    : IIntegerWriter
{
    private RightSideBufferWriter<char> _writer = new(buffer);
    private readonly int _groupLength = groupLength;

    private void Write(IntegerInfo info)
    {
        int bytes = info.ByteSize;
        var bits = info.ValueBits;
        for (int i = 0; i < bytes; i++)
        {
            int shifts = i * 8;
            var byteMask = 0xFFUL << shifts;
            var @byte = (int)((bits & byteMask) >> shifts);
            var left = @byte >> 4;
            var right = @byte & 0xF;
            var leftChar = HexDigitChar(left);
            var rightChar = HexDigitChar(right);
            Write(rightChar);
            Write(leftChar);
        }
    }

    private void Write(char c)
    {
        if (IIntegerWriter.NeedsSeparator(_writer.Length, _groupLength))
        {
            _writer.Write("_");
        }
        _writer.Write(c);
    }

    private static char HexDigitChar(int digit)
    {
        if (digit >= 10)
        {
            return (char)('A' + digit - 10);
        }

        return (char)(digit + '0');
    }

    public string GetString(IntegerInfo info)
    {
        Write(info);
        return _writer.GetFinalized().ToString();
    }

    public static string Write(IntegerInfo info, int groupLength = 0)
    {
        var capacity = CalculateCapacity(info, groupLength);
        Span<char> buffer = stackalloc char[capacity];
        var writer = new HexIntegerWriter(buffer, groupLength);
        return writer.GetString(info);
    }

    private static int CalculateCapacity(IntegerInfo info, int groupLength)
    {
        int valueLength = info.ByteSize * 2;
        int separators = 0;
        if (groupLength > 0)
        {
            separators = (valueLength - 1) / groupLength;
        }

        return valueLength + separators;
    }
}

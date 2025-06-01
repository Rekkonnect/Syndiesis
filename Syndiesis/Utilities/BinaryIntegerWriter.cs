namespace Syndiesis.Utilities;

public ref struct BinaryIntegerWriter(Span<char> buffer, int groupLength = 0)
    : IIntegerWriter
{
    private RightSideBufferWriter<char> _writer = new(buffer);
    private readonly int _groupLength = groupLength;

    private void Write(IntegerInfo info)
    {
        int bitCount = info.BitCount;
        var bits = info.ValueBits;
        for (int i = 0; i < bitCount; i++)
        {
            var bitMask = 1UL << i;
            var bitValue = (bits & bitMask) > 0;
            var bitChar = bitValue ? '1' : '0';
            Write(bitChar);
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

    public string GetString(IntegerInfo info)
    {
        Write(info);
        return _writer.GetFinalized().ToString();
    }

    public static string Write(IntegerInfo info, int groupLength = 0)
    {
        var capacity = CalculateCapacity(info, groupLength);
        Span<char> buffer = stackalloc char[capacity];
        var writer = new BinaryIntegerWriter(buffer, groupLength);
        return writer.GetString(info);
    }

    private static int CalculateCapacity(IntegerInfo info, int groupLength)
    {
        int valueLength = info.BitCount;
        int separators = 0;
        if (groupLength > 0)
        {
            separators = (valueLength - 1) / groupLength;
        }

        return valueLength + separators;
    }
}

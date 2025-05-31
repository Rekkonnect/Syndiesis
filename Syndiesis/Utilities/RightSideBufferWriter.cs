using System.IO;

namespace Syndiesis.Utilities;

public ref struct RightSideBufferWriter<T>(Span<T> buffer)
{
    private readonly Span<T> _buffer = buffer;
    private int _length = 0;

    public int Length => _length;

    private readonly int NextWriteIndex() => _buffer.Length - _length - 1;

    public void Write(T value)
    {
        if (_length >= _buffer.Length)
        {
            throw new InternalBufferOverflowException("The buffer is full.");
        }

        WriteUnsafe(value);
    }

    public void Write(ReadOnlySpan<T> span)
    {
        int nextLength = span.Length + _length;
        if (nextLength > _buffer.Length)
        {
            throw new InternalBufferOverflowException("The buffer has insufficient free capacity.");
        }

        WriteUnsafe(span);
    }

    public unsafe void WriteUnsafe(T value)
    {
        _buffer[NextWriteIndex()] = value;
        _length++;
    }

    public unsafe void WriteUnsafe(ReadOnlySpan<T> span)
    {
        int writtenLength = span.Length;
        int extra = writtenLength - 1;
        int start = NextWriteIndex() - extra;
        var target = _buffer.Slice(start, writtenLength);
        span.CopyTo(target);
        _length += writtenLength;
    }

    public readonly Span<T> GetFinalized()
    {
        int start = NextWriteIndex() + 1;
        return _buffer[start..];
    }
}

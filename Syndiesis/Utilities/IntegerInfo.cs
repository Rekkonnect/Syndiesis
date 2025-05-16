using Garyon.Reflection;
using System;
using System.Numerics;

namespace Syndiesis.Utilities;

/// <summary>
/// Contains information about an integer value that was derived from an object.
/// </summary>
/// <param name="Value">The original value as an object as it was retrieved.</param>
/// <param name="ValueBits">The bits of the integer value in a 64-bit integer.</param>
/// <param name="ByteSize">The number of bytes the integer has.</param>
/// <remarks>
/// This only supports up to <see cref="ulong"/>. Larger integers are not
/// natively implemented and are thus ignored.
/// </remarks>
public readonly record struct IntegerInfo(
    object Value,
    ulong ValueBits,
    int ByteSize)
{
    public const int MaxBits = sizeof(ulong) * 8;

    public TypeCode TypeCode => Value.GetType().GetTypeCode();

    /// <summary>
    /// Gets the number of bits excluding the leading zeroes.
    /// </summary>
    public int BitCount => MaxBits - BitOperations.LeadingZeroCount(ValueBits);

    public static IntegerInfo Create(object value)
    {
        switch (value.GetType().GetTypeCode())
        {
            case TypeCode.SByte:
            {
                var @sbyte = (sbyte)value;
                var @byte = unchecked((byte)@sbyte);
                ulong bits = @byte;
                return new IntegerInfo(value, bits, sizeof(sbyte));
            }
            case TypeCode.Byte:
            {
                var @byte = (byte)value;
                ulong bits = @byte;
                return new IntegerInfo(value, bits, sizeof(byte));
            }

            case TypeCode.Int16:
            {
                var @short = (short)value;
                var @ushort = unchecked((ushort)@short);
                ulong bits = @ushort;
                return new IntegerInfo(value, bits, sizeof(short));
            }
            case TypeCode.UInt16:
            {
                var @ushort = (ushort)value;
                ulong bits = @ushort;
                return new IntegerInfo(value, bits, sizeof(ushort));
            }

            case TypeCode.Int32:
            {
                var @int = (int)value;
                var @uint = unchecked((uint)@int);
                ulong bits = @uint;
                return new IntegerInfo(value, bits, sizeof(int));
            }
            case TypeCode.UInt32:
            {
                var @uint = (uint)value;
                ulong bits = @uint;
                return new IntegerInfo(value, bits, sizeof(uint));
            }

            case TypeCode.Int64:
            {
                var @long = (long)value;
                var @ulong = unchecked((ulong)@long);
                ulong bits = @ulong;
                return new IntegerInfo(value, bits, sizeof(long));
            }
            case TypeCode.UInt64:
            {
                var @ulong = (ulong)value;
                ulong bits = @ulong;
                return new IntegerInfo(value, bits, sizeof(ulong));
            }
        }

        throw new NotSupportedException("The object type is not supported.");
    }
}

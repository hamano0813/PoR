namespace PoR.Core.Values;

/// <summary>
/// Big-endian field codec mirroring the original Python <c>Value</c> class.
/// A positive length is unsigned; a negative length is signed.
/// Bit fields are always read as unsigned bit slices.
/// </summary>
public static class ValueCodec
{
    public static (long Min, long Max) GetRange(int length, BitField? bit = null)
    {
        if (bit is { } bitField)
        {
            bitField.Validate(Math.Abs(length));
            if (bitField.Width >= 64)
            {
                return (0, long.MaxValue);
            }

            return (0, (1L << bitField.Width) - 1L);
        }

        var byteCount = Math.Abs(length);
        ValidateByteCount(byteCount);

        if (length > 0)
        {
            // 8-byte unsigned values cannot be represented by long.
            if (byteCount == 8)
            {
                return (0, long.MaxValue);
            }

            return (0, (1L << (byteCount * 8)) - 1L);
        }

        if (byteCount == 8)
        {
            return (long.MinValue, long.MaxValue);
        }

        var half = 1L << ((byteCount * 8) - 1);
        return (-half, half - 1);
    }

    public static long Read(ReadOnlySpan<byte> buffer, int length, BitField? bit = null)
    {
        var byteCount = Math.Abs(length);
        ValidateByteCount(byteCount);

        if (buffer.Length < byteCount)
        {
            throw new ArgumentException(
                $"Buffer has {buffer.Length} bytes, field needs {byteCount}.",
                nameof(buffer));
        }

        var raw = ReadRaw(buffer, byteCount);

        if (bit is { } bitField)
        {
            bitField.Validate(byteCount);
            return (long)((raw & bitField.Mask) >> bitField.Start);
        }

        if (length < 0)
        {
            var bitCount = byteCount * 8;
            if (bitCount < 64 && (raw & (1UL << (bitCount - 1))) != 0)
            {
                raw |= ulong.MaxValue << bitCount;
            }

            return unchecked((long)raw);
        }

        if (raw > long.MaxValue)
        {
            throw new OverflowException(
                $"Unsigned {byteCount}-byte value 0x{raw:X} does not fit in Int64.");
        }

        return (long)raw;
    }

    public static void Write(Span<byte> buffer, int length, long value, BitField? bit = null)
    {
        var byteCount = Math.Abs(length);
        ValidateByteCount(byteCount);

        if (buffer.Length < byteCount)
        {
            throw new ArgumentException(
                $"Buffer has {buffer.Length} bytes, field needs {byteCount}.",
                nameof(buffer));
        }

        var (min, max) = GetRange(length, bit);
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Value must be in [{min}, {max}] for length {length}" +
                (bit is null ? "." : $" and bit range [{bit.Value.Start}, {bit.Value.End})."));
        }

        if (bit is { } bitField)
        {
            bitField.Validate(byteCount);
            var raw = ReadRaw(buffer, byteCount);
            raw = (raw & ~bitField.Mask) |
                  (((ulong)value << bitField.Start) & bitField.Mask);
            WriteRaw(buffer, byteCount, raw);
            return;
        }

        var encoded = length < 0
            ? unchecked((ulong)value)
            : (ulong)value;
        WriteRaw(buffer, byteCount, encoded);
    }

    private static ulong ReadRaw(ReadOnlySpan<byte> buffer, int byteCount)
    {
        ulong raw = 0;
        for (var index = 0; index < byteCount; index++)
        {
            raw = (raw << 8) | buffer[index];
        }

        return raw;
    }

    private static void WriteRaw(Span<byte> buffer, int byteCount, ulong raw)
    {
        for (var index = byteCount - 1; index >= 0; index--)
        {
            buffer[index] = (byte)(raw & 0xFF);
            raw >>= 8;
        }
    }

    private static void ValidateByteCount(int byteCount)
    {
        if (byteCount is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteCount),
                byteCount,
                "Field length must be 1..8 bytes.");
        }
    }
}

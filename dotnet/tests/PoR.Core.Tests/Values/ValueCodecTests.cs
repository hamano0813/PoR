using PoR.Core.Values;

namespace PoR.Core.Tests.Values;

public sealed class ValueCodecTests
{
    [Theory]
    [InlineData(1, 0xFF, 255)]
    [InlineData(2, 0x1234, 4660)]
    [InlineData(3, 0xABCDEF, 11_259_375)]
    [InlineData(4, 0xFFFFFFFF, 4_294_967_295L)]
    public void ReadsUnsignedBigEndian(int length, long raw, long expected)
    {
        var buffer = ToBuffer(raw, length);
        Assert.Equal(expected, ValueCodec.Read(buffer, length));
    }

    [Theory]
    [InlineData(-1, 0xFF, -1)]
    [InlineData(-1, 0x80, -128)]
    [InlineData(-2, 0x8000, -32768)]
    [InlineData(-4, 0xFFFFFFFF, -1)]
    public void ReadsSignedBigEndian(int length, long raw, long expected)
    {
        var buffer = ToBuffer(raw, Math.Abs(length));
        Assert.Equal(expected, ValueCodec.Read(buffer, length));
    }

    [Theory]
    [InlineData(1, 255)]
    [InlineData(2, 0x1234)]
    [InlineData(3, 0xABCDEF)]
    [InlineData(4, 4_294_967_295L)]
    public void WritesUnsignedBigEndian(int length, long value)
    {
        var buffer = new byte[length];
        ValueCodec.Write(buffer, length, value);
        Assert.Equal(value, ValueCodec.Read(buffer, length));
        Assert.Equal(ToBuffer(value, length), buffer);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1, -128)]
    [InlineData(-1, 127)]
    [InlineData(-2, -32768)]
    [InlineData(-4, int.MinValue)]
    public void WritesSignedBigEndian(int length, long value)
    {
        var buffer = new byte[Math.Abs(length)];
        ValueCodec.Write(buffer, length, value);
        Assert.Equal(value, ValueCodec.Read(buffer, length));
    }

    [Fact]
    public void SingleBitReadAndWritePreservesNeighbors()
    {
        var buffer = new byte[] { 0b1010_1010 };
        var bit = new BitField(3, 4);

        Assert.Equal(1, ValueCodec.Read(buffer, 1, bit));

        ValueCodec.Write(buffer, 1, 0, bit);
        Assert.Equal(0b1010_0010, buffer[0]);

        ValueCodec.Write(buffer, 1, 1, bit);
        Assert.Equal(0b1010_1010, buffer[0]);
    }

    [Fact]
    public void MultiBitReadAndWritePreservesNeighbors()
    {
        var buffer = new byte[] { 0b1011_0110 };
        var bit = new BitField(1, 4);

        Assert.Equal(3, ValueCodec.Read(buffer, 1, bit));

        ValueCodec.Write(buffer, 1, 5, bit);
        Assert.Equal(0b1011_1010, buffer[0]);
        Assert.Equal(5, ValueCodec.Read(buffer, 1, bit));
    }

    [Theory]
    [InlineData(1, 0, 255)]
    [InlineData(-1, -128, 127)]
    [InlineData(2, 0, 65535)]
    [InlineData(-2, -32768, 32767)]
    [InlineData(3, 0, 16_777_215)]
    [InlineData(4, 0, 4_294_967_295L)]
    public void ComputesNumericRanges(int length, long expectedMin, long expectedMax)
    {
        var (min, max) = ValueCodec.GetRange(length);
        Assert.Equal(expectedMin, min);
        Assert.Equal(expectedMax, max);
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(0, 4, 15)]
    [InlineData(3, 4, 1)]
    [InlineData(4, 8, 15)]
    public void ComputesBitRanges(int start, int end, long expectedMax)
    {
        var (min, max) = ValueCodec.GetRange(1, new BitField(start, end));
        Assert.Equal(0, min);
        Assert.Equal(expectedMax, max);
    }

    [Fact]
    public void RejectsOutOfRangeUnsignedWrite()
    {
        var buffer = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ValueCodec.Write(buffer, 1, 256));
    }

    [Fact]
    public void RejectsOutOfRangeBitWrite()
    {
        var buffer = new byte[1];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ValueCodec.Write(buffer, 1, 2, new BitField(0, 1)));
    }

    [Fact]
    public void RejectsShortBuffer()
    {
        Assert.Throws<ArgumentException>(
            () => ValueCodec.Read(new byte[1], 2));
    }

    private static byte[] ToBuffer(long value, int length)
    {
        var buffer = new byte[length];
        for (var index = length - 1; index >= 0; index--)
        {
            buffer[index] = (byte)(value & 0xFF);
            value >>= 8;
        }

        return buffer;
    }
}

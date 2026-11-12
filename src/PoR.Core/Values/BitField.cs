namespace PoR.Core.Values;

/// <summary>
/// A bit range inside a field. <see cref="End"/> is exclusive.
/// A single-bit field is represented as (bit, bit + 1).
/// </summary>
public readonly record struct BitField(int Start, int End)
{
    public int Width => End - Start;

    public ulong Mask
    {
        get
        {
            if (Width <= 0)
            {
                throw new InvalidOperationException($"Invalid bit width {Width}.");
            }

            if (Width >= 64)
            {
                return ulong.MaxValue;
            }

            return ((1UL << Width) - 1UL) << Start;
        }
    }

    public void Validate(int fieldLength)
    {
        if (fieldLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldLength));
        }

        var bitCount = fieldLength * 8;
        if (Start < 0 || End <= Start || End > bitCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(End),
                $"Bit range [{Start}, {End}) is outside a {bitCount}-bit field.");
        }
    }
}

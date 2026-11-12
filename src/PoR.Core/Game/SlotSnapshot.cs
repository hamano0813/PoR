using PoR.Core.Values;

namespace PoR.Core.Game;

/// <summary>
/// One bulk read of the 159 character slots.
/// </summary>
public sealed class SlotSnapshot
{
    public SlotSnapshot(
        uint baseAddress,
        int slotCount,
        int slotStep,
        ReadOnlyMemory<byte> data)
    {
        if (slotCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotCount));
        }

        if (slotStep <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slotStep));
        }

        if (data.Length < slotCount * slotStep)
        {
            throw new ArgumentException(
                $"Snapshot has {data.Length} bytes, needs {slotCount * slotStep}.",
                nameof(data));
        }

        BaseAddress = baseAddress;
        SlotCount = slotCount;
        SlotStep = slotStep;
        Data = data;
    }

    public uint BaseAddress { get; }

    public int SlotCount { get; }

    public int SlotStep { get; }

    public ReadOnlyMemory<byte> Data { get; }

    public ReadOnlySpan<byte> GetSlot(int slotIndex)
    {
        ValidateSlot(slotIndex);
        return Data.Span.Slice(slotIndex * SlotStep, SlotStep);
    }

    public long ReadField(FieldDefinition field, int slotIndex)
    {
        if (field.Scope != FieldScope.Character)
        {
            throw new ArgumentException(
                $"Field '{field.Id}' is not a character field.",
                nameof(field));
        }

        ValidateSlot(slotIndex);

        var fieldOffset = (long)field.Address - BaseAddress;
        if (fieldOffset < 0 || fieldOffset + field.ByteLength > SlotStep)
        {
            throw new ArgumentOutOfRangeException(
                nameof(field),
                $"Field '{field.Id}' is outside the slot template.");
        }

        var offset = (slotIndex * SlotStep) + (int)fieldOffset;
        var buffer = Data.Span.Slice(offset, field.ByteLength);
        return ValueCodec.Read(buffer, field.Length, field.Bit);
    }

    public uint ReadUInt32(int slotIndex, int offset)
    {
        ValidateSlot(slotIndex);
        if (offset < 0 || offset + sizeof(uint) > SlotStep)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        var buffer = Data.Span.Slice((slotIndex * SlotStep) + offset, sizeof(uint));
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    private void ValidateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        }
    }
}

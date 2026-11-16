using PoR.Core.Values;

namespace PoR.Core.Game;

/// <summary>
/// One bulk read of the 159 character slots.
/// </summary>
public sealed class SlotSnapshot
{
    private readonly byte[] _data;

    public SlotSnapshot(
        uint baseAddress,
        int slotCount,
        int slotStep,
        byte[] data)
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
        _data = data;
    }

    public uint BaseAddress { get; }

    public int SlotCount { get; }

    public int SlotStep { get; }

    public ReadOnlyMemory<byte> Data => _data;

    public ReadOnlySpan<byte> GetSlot(int slotIndex)
    {
        ValidateSlot(slotIndex);
        return _data.AsSpan(slotIndex * SlotStep, SlotStep);
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
        var buffer = _data.AsSpan(offset, field.ByteLength);
        return ValueCodec.Read(buffer, field.Length, field.Bit);
    }

    /// <summary>
    /// Applies a successful memory write to the snapshot so that switching
    /// characters reuses the edited value instead of the refresh-time value.
    /// </summary>
    public void WriteField(FieldDefinition field, int slotIndex, long value)
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
        ValueCodec.Write(
            _data.AsSpan(offset, field.ByteLength),
            field.Length,
            value,
            field.Bit);
    }

    public uint ReadUInt32(int slotIndex, int offset)
    {
        ValidateSlot(slotIndex);
        if (offset < 0 || offset + sizeof(uint) > SlotStep)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        var buffer = _data.AsSpan((slotIndex * SlotStep) + offset, sizeof(uint));
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

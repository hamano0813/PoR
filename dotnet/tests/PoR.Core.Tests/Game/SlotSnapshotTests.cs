using PoR.Core.Game;
using PoR.Core.Values;

namespace PoR.Core.Tests.Game;

public sealed class SlotSnapshotTests
{
    private const uint BaseAddress = 0x802AF5E4;
    private const int Step = 0x10;

    [Fact]
    public void ReadsFieldsFromTheSelectedSlot()
    {
        var data = new byte[Step * 2];
        data[0x04] = 0x12;
        data[0x05] = 0x34;
        data[Step + 0x04] = 0xAB;
        data[Step + 0x05] = 0xCD;

        var snapshot = new SlotSnapshot(BaseAddress, 2, Step, data);
        var field = Field(
            "test",
            BaseAddress + 0x04,
            2,
            null,
            FieldScope.Character,
            FieldKind.Numeric);

        Assert.Equal(0x1234, snapshot.ReadField(field, 0));
        Assert.Equal(0xABCD, snapshot.ReadField(field, 1));
    }

    [Fact]
    public void ReadsBitField()
    {
        var data = new byte[Step];
        data[0x02] = 0b1010_1010;

        var snapshot = new SlotSnapshot(BaseAddress, 1, Step, data);
        var field = Field(
            "flags",
            BaseAddress + 0x02,
            1,
            new BitField(3, 4),
            FieldScope.Character,
            FieldKind.Bool);

        Assert.Equal(1, snapshot.ReadField(field, 0));
    }

    [Fact]
    public void RejectsFieldOutsideSlot()
    {
        var snapshot = new SlotSnapshot(BaseAddress, 1, Step, new byte[Step]);
        var field = Field(
            "outside",
            BaseAddress + Step,
            1,
            null,
            FieldScope.Character,
            FieldKind.Numeric);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => snapshot.ReadField(field, 0));
    }

    [Fact]
    public void RejectsGlobalField()
    {
        var snapshot = new SlotSnapshot(BaseAddress, 1, Step, new byte[Step]);
        var field = Field(
            "money",
            0x8032E7A1,
            3,
            null,
            FieldScope.Global,
            FieldKind.Numeric);

        Assert.Throws<ArgumentException>(() => snapshot.ReadField(field, 0));
    }

    [Fact]
    public void ReadUInt32UsesBigEndian()
    {
        var data = new byte[Step];
        data[0x08] = 0x12;
        data[0x09] = 0x34;
        data[0x0A] = 0x56;
        data[0x0B] = 0x78;

        var snapshot = new SlotSnapshot(BaseAddress, 1, Step, data);
        Assert.Equal(0x12345678u, snapshot.ReadUInt32(0, 0x08));
    }

    private static FieldDefinition Field(
        string id,
        uint address,
        int length,
        BitField? bit,
        FieldScope scope,
        FieldKind kind)
        => new(
            id,
            id,
            address,
            length,
            bit,
            scope,
            kind,
            null,
            false,
            false,
            $"Ui.Field.{id}");
}
